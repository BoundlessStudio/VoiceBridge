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
    await Shell.Current.GoToAsync(nameof(BridgePage));
  }

  private async void OnSettingsClicked(object sender, EventArgs e)
  {
    await Shell.Current.GoToAsync(nameof(SettingsPage));
  }

}