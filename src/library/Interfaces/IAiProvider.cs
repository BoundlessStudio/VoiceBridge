namespace VoiceBridge.Interfaces;

public interface IAiProvider
{
  public Task<string> GetAIResponseAsync(string text);
  public void ClearMessages();
}
