#if IOS
using System.Runtime.InteropServices;
using AVFoundation;
using Foundation;
using VoiceBridge.App.Services;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;

namespace VoiceBridge.App.Devices;

public class AudioRecorder : IAudioRecorder, IDisposable
{
    private const int SAMPLE_RATE = 24_100;
    private AVAudioEngine audioEngine;
    private AVAudioFormat audioFormat;
    private volatile bool isRecording;
    private readonly ISettingsService settings;
    private readonly IActivityDetector activityDetector;
    private readonly List<BinaryData> activityBuffer;

    public event EventHandler<SpeechAvailableEventArgs>? OnSpeechDetected;
    public bool IsRecording => isRecording;

    public AudioRecorder(ISettingsService settings, IActivityDetector activityDetector)
    {
        this.settings = settings;
        this.activityDetector = activityDetector;
        this.activityBuffer = new List<BinaryData>();

        var audioSession = AVAudioSession.SharedInstance();
        audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);
        audioSession.SetActive(true);

        
        audioEngine = new AVAudioEngine();
        audioFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, SAMPLE_RATE, 1, false);
    }

    public void Start()
    {
        if (isRecording) return;

        var inputNode = audioEngine.InputNode;
        inputNode.InstallTapOnBus(0, 4096, audioFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) =>
        {
            ProcessAudioData(buffer);
        });

        audioEngine.Prepare();
        audioEngine.StartAndReturnError(out NSError error);

        if (error != null)
        {
            Console.WriteLine($"Error starting audio engine: {error.LocalizedDescription}");
            return;
        }

        isRecording = true;
    }

    public void Stop()
    {
        if (!isRecording) return;

        audioEngine.InputNode.RemoveTapOnBus(0);
        audioEngine.Stop();
        isRecording = false;
        activityBuffer.Clear();
    }

    private void ProcessAudioData(AVAudioPcmBuffer buffer)
    {
        if (buffer.Int16ChannelData == IntPtr.Zero) return;

        int bytesPerFrame = 2; // 16-bit audio
        int dataSize = (int)buffer.FrameLength * bytesPerFrame;
        byte[] audioData = new byte[dataSize];

        // Safe method to copy data from unmanaged memory
        Marshal.Copy(buffer.Int16ChannelData, audioData, 0, dataSize);

        var data = BinaryData.FromBytes(audioData);
        var options = new ActivityDetectorOptions
        {
            VolumeThreshold = settings.GetVolumeThreshold()
        };

        bool activityDetected = activityDetector.ActivityDetected(data, options);

        if (activityDetected)
        {
            activityBuffer.Add(data);
        }
        else if (activityBuffer.Count > 5)
        {
            var bufferCopy = new List<BinaryData>(activityBuffer);
            OnSpeechDetected?.Invoke(this, new SpeechAvailableEventArgs(bufferCopy));
            activityBuffer.Clear();
        }
        else
        {
            activityBuffer.Clear();
        }
    }

    public void Dispose()
    {
        audioEngine?.Dispose();
        audioFormat?.Dispose();
    }
}

#endif