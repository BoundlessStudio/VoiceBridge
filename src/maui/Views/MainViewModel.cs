using VoiceBridge.App.Services;

namespace VoiceBridge.App.Views;

public class MainViewModel : BindableObject
{
  //private int _count;

  //public int Count
  //{
  //  get => _count;
  //  set
  //  {
  //    _count = value;
  //    OnPropertyChanged();
  //    OnPropertyChanged(nameof(CounterText));
  //  }
  //}

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

  //public string CounterText => $"Clicked {Count} time{(Count == 1 ? "" : "s")}";

  //public ICommand IncrementCommand { get; }
  //public ICommand ResetCommand { get; }

  public MainViewModel()
  {
    //this.ConnectVisable = !string.IsNullOrEmpty(settings.OpenAiKey);
    //this.IncrementCommand = new Command(() => Count++);
    //this.ResetCommand = new Command(() => Count = 0);
  }
}