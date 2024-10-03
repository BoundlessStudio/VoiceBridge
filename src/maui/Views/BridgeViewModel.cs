using CommunityToolkit.Mvvm.Input;
using OpenAI.Audio;
using System.Collections.ObjectModel;
using VoiceBridge.App.Services;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;

namespace VoiceBridge.App.Views;

public class BridgeViewModel : BindableObject
{
  private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

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

  private string location { get; set; }

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
    this.location = "Unknown";
  }

  public async Task StartAsync()
  {
    if (this.IsActive == true) return;
    this.IsActive = true;
    this.IsInactive = false;
    this.LogEntries.Clear();
    this.aiProvider.ClearMessages();
    this.recorder.OnSpeechDetected += Recorder_OnSpeechDetected;
    await this.recorder.StartAsync();
    this.player.Start();

    var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
    if (status != PermissionStatus.Granted)
      return;

    var location = await Geolocation.Default.GetLocationAsync();
    if (location is null)
      return;
    
    var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);

    var placemark = placemarks?.FirstOrDefault();
    if (placemark is null)
      return;

    this.location = $"${placemark.Locality}, {placemark.CountryName}";
  }

  public async Task StopAsync()
  {
    if (this.IsActive == false) return;

    this.IsActive = false;
    this.IsInactive = true;
    this.recorder.OnSpeechDetected -= Recorder_OnSpeechDetected;
    await this.recorder.StopAsync();
    this.player.Stop();
  }

  public async void Recorder_OnSpeechDetected(object? sender, SpeechAvailableEventArgs e)
  {
    player.Stop();

    // Buffer Clear Delay
    await Task.Delay(50);

    var recording = this.sttProvider.CreateWaveFile(e.Buffer);
    var transcript = await this.sttProvider.TranscribeAudioAsync(recording);
    if (transcript is null)
      return;

    LogEntries.Insert(0, transcript);

    var aiOptions = new AiProviderOptions()
    {
      Instructions = this.settings.Instructions,
      Location = this.location,
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
    var voice = await this.ttsProvider.GenerateSpeechFromTextAsync(response, voiceOptions);
    if (voice is null)
      return;

    await _lock.WaitAsync();
    try
    {
      await player.WriteAsync(voice);
    }
    finally
    {
      _lock.Release();
    }
  }
}