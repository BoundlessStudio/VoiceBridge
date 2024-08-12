
using Microsoft.Extensions.Logging;
using VoiceBridge.Interfaces;

namespace VoiceBridge;


public class SpeechPipeline : ISpeechPipeline
{
  private readonly ILogger logger;
  private readonly IRecordingDevice recordingDevice;
  private readonly ISpeechToTextProvider sttProvider;
  private readonly ITextToSpeechProvider ttsProvider;
  private readonly IAiProvider aiProvider;
  private readonly IPlaybackDevice playbackDevice;

  public SpeechPipeline(ILogger logger, IRecordingDevice recordingDevice, ISpeechToTextProvider sttProvider, ITextToSpeechProvider ttsProvider, IAiProvider aiProvider, IPlaybackDevice playbackDevice)
  {
    this.logger = logger;
    this.recordingDevice = recordingDevice;
    this.recordingDevice.RecordingCaptured += async (sender, e) => await RecordingCaptured(sender, e);
    this.sttProvider = sttProvider;
    this.ttsProvider = ttsProvider;
    this.aiProvider = aiProvider;
    this.playbackDevice = playbackDevice;
  }

  private async Task RecordingCaptured(object? sender, RecordingCapturedEventArgs e)
  {
    try
    {
      var speech = await recordingDevice.WriteRecording(e.Data);
      var transcript = await this.sttProvider.TranscribeAudioAsync(speech);
      if (transcript is null) 
        return;

      // TODO: Transcript buffer
      // Dose transcript start with call sign?
      // Dose transcript end with sign off (over/roger/73)?

      var response = await this.aiProvider.GetAIResponseAsync(transcript);
      var voice = await this.ttsProvider.GenerateSpeechFromTextAsync(response);
      await this.playbackDevice.PlayAudioAsync(voice);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error in the Speech pipeline.");
    }
  }

  public void Clear() => this.aiProvider.ClearMessages();
  public void Start() => this.recordingDevice.Start();
  public void Stop() => this.recordingDevice.Stop();
}
