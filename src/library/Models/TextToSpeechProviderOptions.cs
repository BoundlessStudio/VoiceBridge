using OpenAI.Audio;
using System.Text.Json.Serialization;

namespace VoiceBridge.Models;

public class TextToSpeechProviderOptions
{
  public GeneratedSpeechVoice Voice { get; set; } = GeneratedSpeechVoice.Alloy;
  public bool AddVoxTrigger { get; set; }

}
