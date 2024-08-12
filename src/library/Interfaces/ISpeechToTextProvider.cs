namespace VoiceBridge.Interfaces;

public interface ISpeechToTextProvider
{
    public Task<string?> TranscribeAudioAsync(Stream speech); // string filename = "speech.wav"
}
