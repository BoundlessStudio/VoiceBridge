namespace VoiceBridge.App
{
  public partial class AppShell : Shell
  {
    public AppShell()
    {
      InitializeComponent();

      Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
      Routing.RegisterRoute(nameof(BridgePage), typeof(BridgePage));
      Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
  }
}
