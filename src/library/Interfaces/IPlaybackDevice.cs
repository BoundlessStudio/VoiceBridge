namespace VoiceBridge.Interfaces;

public interface IPlaybackDevice
{
  public bool IsPlaying { get; }
  void Start();
  void Stop();
  void Flush();
  void Write(BinaryData audioData);
  Task WriteAsync(BinaryData data);
}
