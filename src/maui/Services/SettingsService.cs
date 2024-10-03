using VoiceBridge.AIProvider;

namespace VoiceBridge.App.Services;

public interface ISettingsService
{
  string OpenAiKey { get; set; }
  string Instructions { get; set; }
  int VolumeThreshold { get; set; }
  bool VoxTrigger { get; set; }
  string Voice { get; set; }

  void ResetInstructions();
  float GetVolumeThreshold();
}

public class SettingsService : ISettingsService
{
  private const string OpenAiKeyKey = "OpenAiKey";
  private const string InstructionsKey = "Instructions";
  private const string VolumeThresholdKey = "VolumeThreshold";
  private const string VoxTriggerKey = "VoxTrigger";
  private const string VoiceKey = "Voice";

  public string OpenAiKey
  {
    get => Preferences.Get(OpenAiKeyKey, "");
    set => Preferences.Set(OpenAiKeyKey, value);
  }
  public string Instructions
  {
    get => Preferences.Get(InstructionsKey, OpenAiProvider.DefaultInstructions);
    set => Preferences.Set(InstructionsKey, value);
  }

  public void ResetInstructions()
  {
    Preferences.Set(InstructionsKey, OpenAiProvider.DefaultInstructions);
  }

  public float GetVolumeThreshold()
  {
    switch (VolumeThreshold)
    {
      case 2:
        return 0.1f;
      case 1:
        return 0.05f;
      case 0:
      default:
        return 0.005f;
    }
  }

  public int VolumeThreshold
  {
    get => Preferences.Get(VolumeThresholdKey, 0);
    set => Preferences.Set(VolumeThresholdKey, value);
  }

  public bool VoxTrigger
  {
    get => Preferences.Get(VoxTriggerKey, true);
    set => Preferences.Set(VoxTriggerKey, value);
  }

  public string Voice
  {
    get => Preferences.Get(VoiceKey, "alloy");
    set => Preferences.Set(VoiceKey, value);
  }
}