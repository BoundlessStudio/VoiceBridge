using VoiceBridge.Models;

namespace VoiceBridge.Interfaces;

public interface IActivityDetector
{
    public bool ActivityDetected(BinaryData data, ActivityDetectorOptions options);
}
