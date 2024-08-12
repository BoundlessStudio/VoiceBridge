using System.Text.Json.Serialization;

namespace VoiceBridge.Models;

public class VoiceBridgeOptions
{
  public string OpenAiKey { get; set; }
  public string AiCallSign { get; set; }
  public string UserCallSign { get; set; }

  [JsonConstructor]
  public VoiceBridgeOptions(string key, string ai, string user)
  {
    this.OpenAiKey = key;
    this.AiCallSign = ai;
    this.UserCallSign = user;
  }
}
