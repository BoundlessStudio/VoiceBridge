using VoiceBridge.App.Views;

namespace VoiceBridge.App;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
    BindingContext = viewModel;
  }

  private async void OnBackToMainClicked(object sender, EventArgs e)
  {
    await Shell.Current.GoToAsync("..");
    //await Shell.Current.GoToAsync(nameof(MainPage));
  }
}