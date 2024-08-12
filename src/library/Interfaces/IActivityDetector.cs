using NAudio.Wave;

namespace VoiceBridge.Interfaces;

public interface IActivityDetector
{
    public bool ActivityDetected(BinaryData data);
    // build in (isRecording && DateTime.Now > speechEndTime)
}
