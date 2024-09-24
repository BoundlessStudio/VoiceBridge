using OpenAI;
using OpenAI.Audio;
using System.ClientModel;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;

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
      SpeedRatio = 1,
    };
    this.client = client.GetAudioClient("tts-1");
  }

  public async Task<Stream?> StreamSpeechFromTextAsync(string text, TextToSpeechProviderOptions settings)
  {
    // Create the message using the ClientPipeline
    var message = client.Pipeline.CreateMessage();
    var request = message.Request;

    // Set the request URL and method
    request.Method = "POST";
    request.Uri = new Uri("https://api.openai.com/v1/audio/speech");

    // Add the Authorization and Content-Type headers
    request.Headers.Add("Content-Type", "application/json");

    // Set the request body with model, input, and voice as per the OpenAI API
    var body = BinaryData.FromString("{\"model\":\"tts-1\",\"input\":\"" + text + "\",\"voice\":\"alloy\",\"response_format\":\"pcm\"}");
    request.Content = BinaryContent.Create(body);

    // Send the message through the ClientPipeline
    await client.Pipeline.SendAsync(message);

    // Ensure the response is successful
    if (message.Response?.Status != 200)
    {
      throw new Exception($"Error: {message.Response?.Status} - {message.Response?.ReasonPhrase}");
    }

    // Read the response stream as audio
    return message.Response.ContentStream;
  }

  public async Task<BinaryData> GenerateSpeechFromTextAsync(string text, TextToSpeechProviderOptions settings)
  {
    BinaryData response = await client.GenerateSpeechAsync(text, settings.Voice, this.options);
    if (!settings.AddVoxTrigger)
      return response;

    var trigger = GenerateVoxTrigger();
    var data = MergeAudioData(trigger, response);
    return data;
  }

  private static BinaryData GenerateVoxTrigger(int sampleRate = 44100, float duration = 0.05f, float frequency = 1000f)
  {
    int numSamples = (int)(sampleRate * duration);
    float[] samples = new float[numSamples];

    // Generate a sine wave
    for (int i = 0; i < numSamples; i++)
    {
      float t = i / (float)sampleRate;
      samples[i] = (float)Math.Sin(2 * Math.PI * frequency * t);
    }

    // Apply a more pronounced envelope
    for (int i = 0; i < numSamples; i++)
    {
      float normalizedTime = i / (float)numSamples;
      float envelope = (float)(Math.Sin(normalizedTime * Math.PI) * (1 - normalizedTime)); // Curved fade out
      samples[i] *= envelope;
    }

    // Convert to 16-bit PCM
    byte[] byteArray = new byte[numSamples * 2];
    for (int i = 0; i < numSamples; i++)
    {
      short value = (short)(samples[i] * short.MaxValue);
      byte[] bytes = BitConverter.GetBytes(value);
      byteArray[i * 2] = bytes[0];
      byteArray[i * 2 + 1] = bytes[1];
    }

    return BinaryData.FromBytes(byteArray);
  }

  private static BinaryData MergeAudioData(BinaryData popEffect, BinaryData speechAudio)
  {
    byte[] popBytes = popEffect.ToArray();
    byte[] speechBytes = speechAudio.ToArray();

    // Ensure both audio streams are in the same format (16-bit PCM)
    if (popBytes.Length % 2 != 0 || speechBytes.Length % 2 != 0)
    {
      throw new ArgumentException("Audio data must be in 16-bit PCM format");
    }

    using (MemoryStream outputStream = new MemoryStream())
    {
      // Write pop effect
      outputStream.Write(popBytes, 0, popBytes.Length);

      // Write speech audio
      outputStream.Write(speechBytes, 0, speechBytes.Length);

      return BinaryData.FromBytes(outputStream.ToArray());
    }
  }

}
