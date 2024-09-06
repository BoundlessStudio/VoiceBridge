using VoiceBridge.Models;

namespace VoiceBridge.Interfaces;

public interface IAiProvider
{
  public Task<string> GetAIResponseAsync(string text, AiProviderOptions options);
  public void ClearMessages();
}
