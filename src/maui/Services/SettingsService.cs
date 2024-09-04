namespace VoiceBridge.App.Services;

public interface ISettingsService
{
  string OpenAiKey { get; set; }
  string AiCallSign { get; set; }
  string UserCallSign { get; set; }
}

public class SettingsService : ISettingsService
{
  private const string OpenAiKeyKey = "OpenAiKey";
  private const string AiCallSignKey = "AiCallSign";
  private const string UserCallSignKey = "UserCallSign";

  public string OpenAiKey
  {
    get => Preferences.Get(OpenAiKeyKey, "sk-proj-*****************************************");
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
}