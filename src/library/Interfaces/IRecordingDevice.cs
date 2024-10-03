using VoiceBridge.Models;

namespace VoiceBridge.Interfaces;


public interface IAudioRecorder
{
  event EventHandler<SpeechAvailableEventArgs>? OnSpeechDetected;

  bool IsRecording { get; }

  Task StartAsync();
  Task StopAsync();
}
