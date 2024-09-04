namespace VoiceBridge.Interfaces;

public interface ITextToSpeechProvider
{
  Task<BinaryData> GenerateSpeechFromTextAsync(string text);
}
