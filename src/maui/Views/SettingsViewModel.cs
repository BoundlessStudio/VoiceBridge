using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VoiceBridge.App.Services;

namespace VoiceBridge.App.Views;

public class SettingsViewModel : INotifyPropertyChanged
{
  private readonly ISettingsService service;
  

  public SettingsViewModel(ISettingsService settingsService)
  {
    service = settingsService;
    this.ResetInstructionsCommand = new Command(ResetInstructions);
  }

  public ICommand ResetInstructionsCommand { get; set; }

  public event PropertyChangedEventHandler? PropertyChanged;

  protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  private void ResetInstructions()
  {
    this.service.ResetInstructions();
  }

  public string OpenAiKey
  {
    get => service.OpenAiKey;
    set
    {
      if (service.OpenAiKey != value)
      {
        service.OpenAiKey = value;
        OnPropertyChanged();
      }
    }
  }

  public string Instructions
  {
    get => service.Instructions;
    set
    {
      if (service.Instructions != value)
      {
        service.Instructions = value;
        OnPropertyChanged();
      }
    }
  }

  public int VolumeThreshold
  {
    get => service.VolumeThreshold;
    set
    {
      if (service.VolumeThreshold != value)
      {
        service.VolumeThreshold = value;
        OnPropertyChanged();
      }
    }
  }

  public bool VoxTrigger
  {
    get => service.VoxTrigger;
    set
    {
      if (service.VoxTrigger != value)
      {
        service.VoxTrigger = value;
        OnPropertyChanged();
      }
    }
  }


  public String Voice
  {
    get => service.Voice;
    set
    {
      if (service.Voice != value)
      {
        service.Voice = value;
        OnPropertyChanged();
      }
    }
  }
}