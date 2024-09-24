using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.Input;
using OpenAI.Audio;
using System.Collections.ObjectModel;
using System.Globalization;
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

  public ObservableCollection<string> LogEntries { get; set; }

  // public string ConnectedText => $"{(IsActive == true ? "Connected to " : "Disconnected from")} the Bridge";

  public IAsyncRelayCommand StartCommand { get; }
  public IAsyncRelayCommand StopCommand { get; }

  private readonly ISettingsService settings;
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
    this.LogEntries = new ObservableCollection<string>();
    this.StartCommand = new AsyncRelayCommand(StartAsync);
    this.StopCommand = new AsyncRelayCommand(StopAsync);

    

    this.settings = settings;
    this.sttProvider = sttProvider;
    this.aiProvider = aiProvider;
    this.ttsProvider = ttsProvider;
    this.recorder = recorder;
    this.player = player;
  }
 
  public async Task StartAsync()
  {
    if (this.IsActive == true) return;

    this.IsActive = true;
    this.IsInactive = false;
    this.LogEntries.Clear();
    this.aiProvider.ClearMessages();
    //this.recorder.OnSpeechDetected += Recorder_OnSpeechDetected;
    //this.recorder.Start();
    //this.player.Start();

    // https://chatgpt.com/c/66db9bac-a320-8001-bd9f-3c450f0052ec

    var isGranted = await SpeechToText.Default.RequestPermissions();
    if (!isGranted)
    {
      return;
    }

    const string beginSpeakingPrompt = "Begin speaking...";

    var RecognitionText = beginSpeakingPrompt;

    var progression = new Progress<string>(txt =>
    {
      if (RecognitionText is beginSpeakingPrompt)
      {
        RecognitionText = string.Empty;
      }

      RecognitionText += txt + " ";
    });
    var recognitionResult = await SpeechToText.Default.ListenAsync(CultureInfo.CurrentCulture, progression);

    if (recognitionResult.IsSuccessful)
    {
      RecognitionText = recognitionResult.Text;
    }
  }

  public async Task StopAsync()
  {
    if (this.IsActive == false) return;

    this.IsActive = false;
    this.IsInactive = true;
    await SpeechToText.Default.StopListenAsync();
    //this.recorder.OnSpeechDetected -= Recorder_OnSpeechDetected;
    //this.recorder.Stop();
    //this.player.Stop();
  }

  private void OnRecognitionTextUpdated(object? sender, SpeechToTextRecognitionResultUpdatedEventArgs args)
  {
    var transcript = args.RecognitionResult;
  }

  private async void OnRecognitionTextCompleted(object? sender, SpeechToTextRecognitionResultCompletedEventArgs args)
  {
    var transcript = args.RecognitionResult;

    LogEntries.Insert(0, transcript);

    var aiOptions = new AiProviderOptions()
    {
      AiCallSign = this.settings.AiCallSign,
      UserCallSign = this.settings.UserCallSign
    };
    var response = await this.aiProvider.GetAIResponseAsync(transcript, aiOptions);
    if (response is null)
      return;

    LogEntries.Insert(0, response);

    await TextToSpeech.Default.SpeakAsync(response);
  }


  public async void Recorder_OnSpeechDetected(object? sender, SpeechAvailableEventArgs e)
  {
    this.player.Flush();

    var recording = this.sttProvider.CreateWaveFile(e.Buffer);
    var transcript = await this.sttProvider.TranscribeAudioAsync(recording);
    if (transcript is null)
      return;

    LogEntries.Insert(0, transcript);

    var aiOptions = new AiProviderOptions()
    {
      AiCallSign = this.settings.AiCallSign,
      UserCallSign = this.settings.UserCallSign
    };
    var response = await this.aiProvider.GetAIResponseAsync(transcript, aiOptions);
    if (response is null)
      return;

    LogEntries.Insert(0, response);

    var voiceOptions = new TextToSpeechProviderOptions()
    {
      Voice = (GeneratedSpeechVoice)settings.Voice,
      AddVoxTrigger = settings.VoxTrigger,
    };
    //var voice = await this.ttsProvider.StreamSpeechFromTextAsync(response, voiceOptions);
    var voice = await this.ttsProvider.GenerateSpeechFromTextAsync(response, voiceOptions);
    if (voice is null)
      return;

    await player.WriteAsync(voice);
  }
}