namespace VoiceBridge.App.Devices;

#if ANDROID
using Android.Media;
using VoiceBridge.Interfaces;

public class AudioPlayer : IPlaybackDevice, IDisposable
{
  private const int SampleRate = 24_100;
  private const Encoding AudioFormat = Android.Media.Encoding.Pcm16bit;

  private AudioTrack player;
  private int bufferSize;

  public bool IsPlaying { get; private set; }

  public AudioPlayer()
  {
    int bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelIn.Mono, AudioFormat);
    this.bufferSize = Math.Max(bufferSize, 4096);

    AudioAttributes audioAttributes = new AudioAttributes.Builder()
       ?.SetUsage(AudioUsageKind.Media)
       ?.SetContentType(AudioContentType.Speech)
       ?.Build() ?? throw new NullReferenceException("AudioFormat.Builder");

    AudioFormat playbackFormat = new AudioFormat.Builder()
        ?.SetEncoding(AudioFormat)
        ?.SetSampleRate(SampleRate)
        ?.SetChannelMask(ChannelOut.Mono)
        ?.Build() ?? throw new NullReferenceException("AudioFormat.Builder");

    player = new AudioTrack.Builder()
        .SetAudioAttributes(audioAttributes)
        .SetAudioFormat(playbackFormat)
        .SetBufferSizeInBytes(bufferSize)
        .SetTransferMode(AudioTrackMode.Stream)
        //.SetPerformanceMode(AudioTrack.PerformanceModeLoadLatency)
        .Build() ?? throw new NullReferenceException("AudioFormat.Builder");

    player.SetVolume(0.5f);
  }


  public void Start()
  {
    player.Play();
  }

  public void Stop()
  {
    player.Stop();
    player.Flush();
  }

  public void Flush()
  {
    player.Stop();
    player.Flush();
    player.Play();
  }

  public void Write(BinaryData data)
  {
    this.IsPlaying = true;
    var buffer = data.ToArray();
    player.Write(buffer, 0, buffer.Length, WriteMode.Blocking);
    this.IsPlaying = false;
  }

  public async Task WriteAsync(BinaryData data)
  {
    this.IsPlaying = true;
    var buffer = data.ToArray();
    await player.WriteAsync(buffer, 0, buffer.Length, WriteMode.Blocking);
    this.IsPlaying = false;
  }

  public void Dispose() => player?.Release();
}
#endif