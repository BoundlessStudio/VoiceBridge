namespace VoiceBridge.Interfaces;

public interface IPlaybackDevice
{
    public bool IsPlaying { get; set; }

    Task PlayAudioAsync(BinaryData audioData);
}
