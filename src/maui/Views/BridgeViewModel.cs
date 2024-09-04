using System.Windows.Input;
using VoiceBridge.App.Services;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;

namespace VoiceBridge.App.Views;

public class BridgeViewModel : BindableObject
{
  private bool _active;

  public bool IsActive
  {
    get => _active;
    set
    {
      _active = value;
      OnPropertyChanged();
      OnPropertyChanged(nameof(IsActive));
    }
  }

  private bool _inactive;
  public bool IsInactive
  {
    get => _inactive;
    set
    {
      _inactive = value;
      OnPropertyChanged();
      OnPropertyChanged(nameof(IsInactive));
    }
  }

  public ICommand StartCommand { get; }
  public ICommand StopCommand { get; }

  private readonly IPlaybackDevice player;
  private readonly IAudioRecorder recorder;
  private readonly ISpeechToTextProvider sttProvider;
  private readonly IAiProvider aiProvider;
  private readonly ITextToSpeechProvider ttsProvider;


  public BridgeViewModel(
    ISettingsService settings,
    ISpeechToTextProvider sttProvider,
    IAiProvider aiProvider,
    ITextToSpeechProvider ttsProvider,
    IAudioRecorder recorder,
    IPlaybackDevice player)
  {
    this.IsActive = false;
    this.IsInactive = true;
    this.StartCommand = new Command(Start);
    this.StopCommand = new Command(Stop);

    this.sttProvider = sttProvider;
    this.aiProvider = aiProvider;
    this.ttsProvider = ttsProvider;
    this.recorder = recorder;
    this.player = player;
  }
 
  public void Start()
  {
    if (this.IsActive == true) return;

    this.IsActive = true;
    this.IsInactive = false;
    this.recorder.OnSpeechDetected += Recorder_OnSpeechDetected;
    this.recorder.Start();
    this.player.Start();
  }

  public void Stop()
  {
    if (this.IsActive == false) return;

    this.IsActive = false;
    this.IsInactive = true;
    this.recorder.OnSpeechDetected -= Recorder_OnSpeechDetected;
    this.recorder.Stop();
    this.player.Stop();
  }

  public async void Recorder_OnSpeechDetected(object? sender, SpeechAvailableEventArgs e)
  {
    var recording = this.sttProvider.CreateWaveFile(e.Buffer);
    var transcript = await this.sttProvider.TranscribeAudioAsync(recording);
    if (transcript is null)
      return;

    var response = await this.aiProvider.GetAIResponseAsync(transcript);
    if (transcript is null)
      return;

    var voice = await this.ttsProvider.GenerateSpeechFromTextAsync(response);
    if (voice is null)
      return;

    player.Write(voice);
  }
}