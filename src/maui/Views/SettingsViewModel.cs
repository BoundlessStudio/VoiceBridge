using System.ComponentModel;
using System.Runtime.CompilerServices;
using VoiceBridge.App.Services;

namespace VoiceBridge.App.Views;

public class SettingsViewModel : INotifyPropertyChanged
{
  private readonly ISettingsService _settingsService;

  public SettingsViewModel(ISettingsService settingsService)
  {
    _settingsService = settingsService;
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public string OpenAiKey
  {
    get => _settingsService.OpenAiKey;
    set
    {
      if (_settingsService.OpenAiKey != value)
      {
        _settingsService.OpenAiKey = value;
        OnPropertyChanged();
      }
    }
  }

  public string AiCallSign
  {
    get => _settingsService.AiCallSign;
    set
    {
      if (_settingsService.AiCallSign != value)
      {
        _settingsService.AiCallSign = value;
        OnPropertyChanged();
      }
    }
  }

  public string UserCallSign
  {
    get => _settingsService.UserCallSign;
    set
    {
      if (_settingsService.UserCallSign != value)
      {
        _settingsService.UserCallSign = value;
        OnPropertyChanged();
      }
    }
  }

  public int VolumeThreshold
  {
    get => _settingsService.VolumeThreshold;
    set
    {
      if (_settingsService.VolumeThreshold != value)
      {
        _settingsService.VolumeThreshold = value;
        OnPropertyChanged();
      }
    }
  }

  public bool VoxTrigger
  {
    get => _settingsService.VoxTrigger;
    set
    {
      if (_settingsService.VoxTrigger != value)
      {
        _settingsService.VoxTrigger = value;
        OnPropertyChanged();
      }
    }
  }


  public int Voice
  {
    get => _settingsService.Voice;
    set
    {
      if (_settingsService.Voice != value)
      {
        _settingsService.Voice = value;
        OnPropertyChanged();
      }
    }
  }
}