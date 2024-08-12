using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Audio;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Voice;

public class OpenAIWhisperProvider : ISpeechToTextProvider
{
  private readonly ILogger logger;
  private readonly AudioTranscriptionOptions options;
  private readonly AudioClient client;

  public OpenAIWhisperProvider(ILogger logger, OpenAIClient client)
  {
    this.logger = logger;
    this.options = new AudioTranscriptionOptions()
    {
      ResponseFormat = AudioTranscriptionFormat.Simple,
    };
    this.client = client.GetAudioClient("whisper-1");
  }

  public async Task<string?> TranscribeAudioAsync(Stream speech)
  {
    AudioTranscription response = await client.TranscribeAudioAsync(speech, "speech.wav", options);
    var transcript = response.Text;
    
    this.logger.LogInformation("Transcript: {transcript}", transcript);

    if (response.Duration < TimeSpan.FromSeconds(1))
    {
      this.logger.LogWarning("Short Circuiting: Duration");
      return null;
    }

    if (!transcript.Any(char.IsLetter))
    {
      this.logger.LogWarning("Short Circuiting: No Words");
      return null;
    }

    return transcript;
  }
}
