using Microsoft.Extensions.Logging;
using NAudio.Wave;
using OpenAI;
using OpenAI.Audio;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Voice;

public class OpenAIVoiceProvider : ITextToSpeechProvider
{
  private readonly ILogger logger;
  private readonly SpeechGenerationOptions options;
  private readonly AudioClient client;

  public OpenAIVoiceProvider(ILogger logger, OpenAIClient client)
  {
    this.logger = logger;
    this.options = new SpeechGenerationOptions()
    {
      ResponseFormat = GeneratedSpeechFormat.Pcm,
      Speed = 1
    };
    this.client = client.GetAudioClient("tts-1");
  }

  public WaveFormat DefaultWaveFormat()
  {
    return new WaveFormat(24000, 16, 1);
  }

  public async Task<BinaryData> GenerateSpeechFromTextAsync(string text)
  {
    BinaryData response = await client.GenerateSpeechFromTextAsync(text, GeneratedSpeechVoice.Alloy, this.options);
    return response;
  }

}
