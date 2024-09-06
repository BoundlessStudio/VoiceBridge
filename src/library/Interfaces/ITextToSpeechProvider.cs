using VoiceBridge.Models;

namespace VoiceBridge.Interfaces;

public interface ITextToSpeechProvider
{
  Task<BinaryData> GenerateSpeechFromTextAsync(string text, TextToSpeechProviderOptions options);
}
