
#if IOS
using VoiceBridge.Interfaces;
using AVFoundation;
using Foundation;

namespace VoiceBridge.App.Devices;

public class AudioPlayer : IPlaybackDevice, IDisposable
{
    private const int SampleRate = 24100;
    private AVAudioEngine audioEngine;
    private AVAudioPlayerNode playerNode;
    private AVAudioFormat audioFormat;

    public bool IsPlaying { get; private set; }

    public AudioPlayer()
    {
        audioEngine = new AVAudioEngine();
        playerNode = new AVAudioPlayerNode();
        audioFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, SampleRate, 1, false);

        audioEngine.AttachNode(playerNode);
        audioEngine.Connect(playerNode, audioEngine.MainMixerNode, audioFormat);

        audioEngine.Prepare();
    }

    public void Start()
    {
        NSError error;
        audioEngine.StartAndReturnError(out error);
        if (error != null)
        {
            Console.WriteLine($"Error starting audio engine: {error.LocalizedDescription}");
        }
    }

    public void Stop()
    {
        playerNode.Stop();
        audioEngine.Stop();
    }

    public void Flush()
    {
        Stop();
        Start();
    }

    public void Write(BinaryData data)
    {
        IsPlaying = true;
        var buffer = data.ToArray();
        var audioBuffer = AudioBufferFromByteArray(buffer);
        playerNode.ScheduleBuffer(audioBuffer, completionHandler: () => 
        {
            IsPlaying = false;
        });
    }

    public async Task WriteAsync(BinaryData data)
    {
        IsPlaying = true;
        var buffer = data.ToArray();
        var audioBuffer = AudioBufferFromByteArray(buffer);
            
        var tcs = new TaskCompletionSource<bool>();
        playerNode.ScheduleBuffer(audioBuffer, completionHandler: () => 
        {
            IsPlaying = false;
            tcs.SetResult(true);
        });
            
        await tcs.Task;
    }

    private AVAudioPcmBuffer AudioBufferFromByteArray(byte[] byteArray)
    {
        // 16-bit PCM, so 2 bytes per frame
        var frameCapacity = byteArray.Length / 2; 
        var pcmBuffer = new AVAudioPcmBuffer(audioFormat, (uint)frameCapacity);

        
        if (pcmBuffer.Int16ChannelData == IntPtr.Zero)
        {
          throw new InvalidOperationException("Failed to create PCM buffer");
        }

        System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, pcmBuffer.Int16ChannelData, byteArray.Length);
        pcmBuffer.FrameLength = (uint)frameCapacity;

        return pcmBuffer;
    }

    public void Dispose()
    {
        playerNode?.Dispose();
        audioEngine?.Dispose();
    }
}

#endif