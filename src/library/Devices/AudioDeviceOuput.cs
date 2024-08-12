using Microsoft.Extensions.Logging;
using NAudio.Wave;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Devices;

public class AudioDeviceOuput : IPlaybackDevice
{
  private readonly WaveFormat waveFormat;
  private readonly ILogger logger;

  public bool IsPlaying { get; set; }

  public AudioDeviceOuput(ILogger logger, ITextToSpeechProvider ttsProvider)
  {
    this.logger = logger;
    this.waveFormat = ttsProvider.DefaultWaveFormat();
  }

  public async Task PlayAudioAsync(BinaryData audioData)
  {
    if (this.IsPlaying)
      return;

    this.logger.LogInformation("Playback started.");

    var tcs = new TaskCompletionSource<bool>();

    using var mainAudioStream = new MemoryStream(audioData.ToArray());
    using var combinedStream = new MemoryStream();

    // Generate and write pop sound
    WritePopSound(combinedStream);

    // Copy main audio
    await mainAudioStream.CopyToAsync(combinedStream);

    combinedStream.Position = 0;

    using var reader = new RawSourceWaveStream(combinedStream, waveFormat);
    var outputDevice = new WaveOutEvent()
    {
      DeviceNumber = 0
    };

    outputDevice.PlaybackStopped += (s, e) =>
    {
      outputDevice.Dispose();
      tcs.TrySetResult(true);
    };
   
    outputDevice.Init(reader);

    this.IsPlaying = true;

    outputDevice.Play();

    await tcs.Task;

    await Task.Delay(1000);

    this.logger.LogInformation("Playback ended.");

    this.IsPlaying = false;
  }

  private void WritePopSound(Stream stream)
  {
    int sampleRate = waveFormat.SampleRate;
    int bitsPerSample = waveFormat.BitsPerSample;
    int channels = waveFormat.Channels;
    int durationMs = 50; // Duration of the pop in milliseconds
    double frequency = 1000; // Frequency of the pop sound in Hz

    int numSamples = sampleRate * channels * durationMs / 1000;

    for (int i = 0; i < numSamples; i++)
    {
      double t = (double)i / (sampleRate * channels);
      double amplitude = Math.Sin(2 * Math.PI * frequency * t);

      // Apply a simple envelope to avoid clicks
      double envelope = Math.Sin(Math.PI * i / numSamples);
      amplitude *= envelope;

      // Scale to 16-bit range and convert to bytes
      short sample = (short)(amplitude * short.MaxValue * 0.25); // 0.25 for lower volume
      byte[] bytes = BitConverter.GetBytes(sample);

      stream.Write(bytes, 0, bytes.Length);
    }
  }

}
