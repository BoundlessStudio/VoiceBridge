using VoiceBridge.App.Views;

namespace VoiceBridge.App;

public partial class BridgePage : ContentPage
{
  private readonly BridgeViewModel viewModel;
  //private readonly IPlaybackDevice player;
  //private readonly IAudioRecorder recorder;
  //private readonly ISpeechToTextProvider sttProvider;
  //private readonly IAiProvider aiProvider;
  //private readonly ITextToSpeechProvider ttsProvider;

  public BridgePage(BridgeViewModel viewModel)
  {
    InitializeComponent();
    this.viewModel = viewModel;
    BindingContext = this.viewModel;

    //this.sttProvider = sttProvider;
    //this.aiProvider = aiProvider;
    //this.ttsProvider = ttsProvider;
    //this.recorder = recorder;
    //this.player = player;
  }

  private async void OnBackToMainClicked(object sender, EventArgs e)
  {
    await Shell.Current.GoToAsync("..");
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();

    this.viewModel.Stop();
  }

  //private async void Recorder_OnSpeechDetected(object? sender, SpeechAvailableEventArgs e)
  //{
  //  var recording = this.sttProvider.CreateWaveFile(e.Buffer);
  //  var transcript = await this.sttProvider.TranscribeAudioAsync(recording);
  //  if (transcript is null)
  //    return;

  //  var response = await this.aiProvider.GetAIResponseAsync(transcript);
  //  if (transcript is null)
  //    return;

  //  var voice = await this.ttsProvider.GenerateSpeechFromTextAsync(response);
  //  if (voice is null)
  //    return;

  //  player.Write(voice);
  //}

  //private void OnStartClicked(object sender, EventArgs e) => Start();

  //private void OnStopClicked(object sender, EventArgs e) => Stop();

  //protected override void OnDisappearing()
  //{
  //  base.OnDisappearing();
  //  Stop();
  //}

  //private void Start()
  //{
  //  if (this.viewModel.isActive == true) return;

  //  this.viewModel.isActive = true;
  //  this.recorder.OnSpeechDetected += Recorder_OnSpeechDetected;
  //  this.recorder.Start();
  //  this.player.Start();
  //}

  //private void Stop()
  //{
  //  if (this.viewModel.isActive == false) return;

  //  this.viewModel.isActive = false;
  //  this.recorder.OnSpeechDetected -= Recorder_OnSpeechDetected;
  //  this.recorder.Stop();
  //  this.player.Stop();
  //}

}