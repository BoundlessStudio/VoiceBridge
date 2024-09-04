using OpenAI;
using OpenAI.Audio;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Voice;

public class OpenAIVoiceProvider : ITextToSpeechProvider
{
  private readonly SpeechGenerationOptions options;
  private readonly AudioClient client;

  public OpenAIVoiceProvider(OpenAIClient client)
  {
    this.options = new SpeechGenerationOptions()
    {
      ResponseFormat = GeneratedSpeechFormat.Pcm,
      Speed = 1
    };
    this.client = client.GetAudioClient("tts-1");
  }

  public async Task<BinaryData> GenerateSpeechFromTextAsync(string text)
  {
    BinaryData response = await client.GenerateSpeechAsync(text, GeneratedSpeechVoice.Alloy, this.options);
    return response;
  }

}
