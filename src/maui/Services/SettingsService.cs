namespace VoiceBridge.App.Services;

public interface ISettingsService
{
  string OpenAiKey { get; set; }
  string AiCallSign { get; set; }
  string UserCallSign { get; set; }
  int VolumeThreshold { get; set; }
  bool VoxTrigger { get; set; }
  int Voice { get; set; }

  float GetVolumeThreshold();
}

public class SettingsService : ISettingsService
{
  private const string OpenAiKeyKey = "OpenAiKey";
  private const string AiCallSignKey = "AiCallSign";
  private const string UserCallSignKey = "UserCallSign";
  private const string VolumeThresholdKey = "VolumeThreshold";
  private const string VoxTriggerKey = "VoxTrigger";
  private const string VoiceKey = "Voice";

  public string OpenAiKey
  {
    get => Preferences.Get(OpenAiKeyKey, "");
    set => Preferences.Set(OpenAiKeyKey, value);
  }

  public string AiCallSign
  {
    get => Preferences.Get(AiCallSignKey, "Assistant");
    set => Preferences.Set(AiCallSignKey, value);
  }

  public string UserCallSign
  {
    get => Preferences.Get(UserCallSignKey, "User");
    set => Preferences.Set(UserCallSignKey, value);
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

  public int Voice
  {
    get => Preferences.Get(VoiceKey, 0);
    set => Preferences.Set(VoiceKey, value);
  }
}