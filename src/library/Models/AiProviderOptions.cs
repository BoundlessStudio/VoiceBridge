using System.Text.Json.Serialization;

namespace VoiceBridge.Models;

public class AiProviderOptions
{
  public string? Instructions { get; set; }
  public string? Location { get; set; }
  public string? AiCallSign { get; set; }
  public string? UserCallSign { get; set; }

}
