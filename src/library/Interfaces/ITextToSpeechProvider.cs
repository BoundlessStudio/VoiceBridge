using NAudio.Wave;
using OpenAI.Audio;

namespace VoiceBridge.Interfaces;

public interface ITextToSpeechProvider
{
  Task<BinaryData> GenerateSpeechFromTextAsync(string text);
  WaveFormat DefaultWaveFormat();
}
