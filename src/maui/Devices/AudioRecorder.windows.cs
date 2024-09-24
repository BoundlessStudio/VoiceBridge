
#if WINDOWS
using Android.Media;
using Android.Media.Audiofx;
using VoiceBridge.App.Services;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;

namespace VoiceBridge.App.Devices;
public class AudioRecorder : IAudioRecorder, IDisposable
{
  private const int SAMPLE_RATE = 24_100;
  private const Encoding AUDIO_FORMAT = Android.Media.Encoding.Pcm16bit;

  private AcousticEchoCanceler? echoCanceler;
  private AudioRecord recorder;
  private readonly int bufferSize;
  private volatile bool isRecording;

  private readonly ISettingsService settings;
  private readonly IActivityDetector activityDetector;
  private readonly List<BinaryData> activityBuffer;
  public event EventHandler<SpeechAvailableEventArgs>? OnSpeechDetected;

  public bool IsRecording => isRecording;

  public AudioRecorder(ISettingsService settings, IActivityDetector activityDetector)
  {
    int bufferSize = AudioRecord.GetMinBufferSize(SAMPLE_RATE, ChannelIn.Mono, AUDIO_FORMAT);
    this.bufferSize = Math.Max(bufferSize, 4096);

    this.isRecording = false;
    this.settings = settings;
    this.activityDetector = activityDetector;
    this.activityBuffer = new List<BinaryData>();

    AudioFormat recordFormat = new AudioFormat.Builder()
       ?.SetEncoding(AUDIO_FORMAT)
       ?.SetSampleRate(SAMPLE_RATE)
       ?.Build() ?? throw new NullReferenceException("AudioFormat.Builder");

    recorder = new AudioRecord.Builder()
        ?.SetAudioSource(AudioSource.Mic)
        ?.SetAudioFormat(recordFormat)
        ?.SetBufferSizeInBytes(bufferSize)
        ?.Build() ?? throw new NullReferenceException(" AudioRecord.Builder");

    if (AcousticEchoCanceler.IsAvailable)
    {
      this.echoCanceler = AcousticEchoCanceler.Create(recorder.AudioSessionId);
      this.echoCanceler?.SetEnabled(true);
    }
  }

  public void Start()
  {
    if (isRecording == true) return;
    isRecording = true;
    recorder.StartRecording();
    Task.Run(RecordAudioData);
  }

  public void Stop()
  {
    if (isRecording == false) return;
    isRecording = false;
    recorder.Stop();
    this.activityBuffer.Clear();
  }

  private void RecordAudioData()
  {
    byte[] buffer = new byte[4096];
    while (isRecording)
    {
      int bytesRead = recorder.Read(buffer, 0, buffer.Length);
      if (bytesRead > 0)
      {
        byte[] audioData = new byte[bytesRead];
        Array.Copy(buffer, audioData, bytesRead);
        var data = BinaryData.FromBytes(audioData);
        ProcessAudioData(data);
      }
    }
  }

  private void ProcessAudioData(BinaryData audioData)
  {
    var options = new ActivityDetectorOptions()
    {
      VolumeThreshold = this.settings.GetVolumeThreshold()
    };
    bool activityDetected = this.activityDetector.ActivityDetected(audioData, options);
    if (activityDetected)
    {
      this.activityBuffer.Add(audioData);
    }
    else if (this.activityBuffer.Count > 5)
    {
      var buffer = this.activityBuffer.ToList();
      this.OnSpeechDetected?.Invoke(this, new SpeechAvailableEventArgs(buffer));
      this.activityBuffer.Clear();
    }
    else
    {
      this.activityBuffer.Clear();
    }
  }

  public void Dispose()
  {
    recorder?.Release();
    echoCanceler?.Release();
  }
}
#endif