using VoiceBridge.Models;

namespace VoiceBridge.Interfaces;


public interface IAudioRecorder
{
  event EventHandler<SpeechAvailableEventArgs>? OnSpeechDetected;

  bool IsRecording { get; }

  void Start();
  void Stop();
}
