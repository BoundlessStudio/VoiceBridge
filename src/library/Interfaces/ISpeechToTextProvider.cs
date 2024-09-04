namespace VoiceBridge.Interfaces;

public interface ISpeechToTextProvider
{
    public Task<string?> TranscribeAudioAsync(BinaryData data);
    BinaryData CreateWaveFile(BinaryData data);
}
