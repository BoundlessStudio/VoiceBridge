using VoiceBridge.App.Services;
using VoiceBridge.App.Views;

namespace VoiceBridge.App;

public partial class MainPage : ContentPage
{
  private readonly MainViewModel viewModel;
  private readonly ISettingsService settings;

  public MainPage(MainViewModel viewModel, ISettingsService settings)
  {
    InitializeComponent();
    this.viewModel = viewModel;
    this.settings = settings;
    BindingContext = this.viewModel;
  }

  protected override void OnAppearing()
  {
    base.OnAppearing();

    this.viewModel.ConnectVisable = !string.IsNullOrEmpty(this.settings.OpenAiKey);
  }


  private async void OnConnectClicked(object sender, EventArgs e)
  {
    PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
    if (status != PermissionStatus.Granted)
      status = await Permissions.RequestAsync<Permissions.Microphone>();

    if (status == PermissionStatus.Granted)
    {
      await Shell.Current.GoToAsync(nameof(BridgePage));
    }
    else
    {
      // await DisplayAlert("Permission Denied", "Microphone permission is required to record audio.", "OK");
    }
  }

  private async void OnSettingsClicked(object sender, EventArgs e)
  {
    await Shell.Current.GoToAsync(nameof(SettingsPage));
  }

}