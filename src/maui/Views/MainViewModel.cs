using VoiceBridge.App.Services;

namespace VoiceBridge.App.Views;

public class MainViewModel : BindableObject
{

  private bool _connectVisable;
  public bool ConnectVisable
  {
    get => _connectVisable;
    set
    {
      _connectVisable = value;
      OnPropertyChanged();
      OnPropertyChanged(nameof(ConnectVisable));
    }
  }


  public MainViewModel()
  {
  }
}