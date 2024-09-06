using OpenAI;
using OpenAI.Audio;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Voice;

public class OpenAIWhisperProvider : ISpeechToTextProvider
{
  private readonly AudioTranscriptionOptions options;
  private readonly AudioClient client;

  public OpenAIWhisperProvider(OpenAIClient client)
  {
    this.options = new AudioTranscriptionOptions()
    {
      ResponseFormat = AudioTranscriptionFormat.Simple,
    };
    this.client = client.GetAudioClient("whisper-1");
  }

  public async Task<string?> TranscribeAudioAsync(BinaryData data)
  {
    var speech = data.ToStream();
    AudioTranscription response = await client.TranscribeAudioAsync(speech, "speech.wav", options);
    var transcript = response.Text;

    if (response.Duration < TimeSpan.FromSeconds(1))
    {
      return null;
    }

    if (!transcript.Any(char.IsLetter))
    {
      return null;
    }

    return transcript;
  }


  public BinaryData CreateWaveFile(BinaryData data)
  {
    var buffer = data.ToArray();

    using (MemoryStream ms = new MemoryStream())
    using (BinaryWriter bw = new BinaryWriter(ms))
    {
      // Write WAV file header
      WriteWaveHeader(bw, buffer.Length);

      bw.Write(buffer);

      // Convert MemoryStream to BinaryData
      return BinaryData.FromBytes(ms.ToArray());
    }
  }

  private const int SAMPLE_RATE = 24_100;
  private const short CHANNELS = 1;         // Mono
  private const short BITS_PER_SAMPLE = 16; // PCM16bit

  private static void WriteWaveHeader(BinaryWriter writer, int totalAudioLen)
  {
    int totalDataLen = totalAudioLen + 36;
    short blockAlign = (short)(CHANNELS * BITS_PER_SAMPLE / 8);
    int avgBytesPerSec = SAMPLE_RATE * blockAlign;

    writer.Write(new char[] { 'R', 'I', 'F', 'F' });
    writer.Write(totalDataLen);
    writer.Write(new char[] { 'W', 'A', 'V', 'E' });
    writer.Write(new char[] { 'f', 'm', 't', ' ' });
    writer.Write(16);               // Sub-chunk size, 16 for PCM
    writer.Write((short)1);         // AudioFormat, 1 for PCM
    writer.Write(CHANNELS);
    writer.Write(SAMPLE_RATE);
    writer.Write(avgBytesPerSec);
    writer.Write(blockAlign);
    writer.Write(BITS_PER_SAMPLE);
    writer.Write(new char[] { 'd', 'a', 't', 'a' });
    writer.Write(totalAudioLen);
  }
}
