using VoiceBridge.App.Views;

namespace VoiceBridge.App;

public partial class BridgePage : ContentPage
{
  private readonly BridgeViewModel viewModel;

  public BridgePage(BridgeViewModel viewModel)
  {
    InitializeComponent();
    this.viewModel = viewModel;
    BindingContext = this.viewModel;
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
}