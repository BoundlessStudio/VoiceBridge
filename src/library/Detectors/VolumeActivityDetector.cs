using Microsoft.Extensions.Logging;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Detectors;

public class VolumeActivityDetector : IActivityDetector
{
    // TODO: Convert to Options
    const float VOLUME_THRESHOLD = 0.002f;
    const int SPEECH_PADDING_MS = 1000;

    private DateTime speechEndTime = DateTime.MinValue;
    private bool isRecording = false;

    private readonly ILogger logger;

    public VolumeActivityDetector(ILogger logger)
    {
        this.logger = logger;
    }

    public bool ActivityDetected(BinaryData data)
    {
        float volume = CalculateVolume(data);
        logger.LogTrace("Volume {v}:F4", volume);

        if (volume > VOLUME_THRESHOLD)
        {
            if (isRecording)
            {
                speechEndTime = DateTime.Now.AddMilliseconds(SPEECH_PADDING_MS);
            }
            else
            {
                isRecording = true;
                logger.LogInformation("Volume Threshold Trigger: {t}:F4", VOLUME_THRESHOLD);
            }
        }
        else if (isRecording && DateTime.Now > speechEndTime)
        {
            isRecording = false;
            logger.LogInformation("Activity Ended");
        }

        return isRecording;
    }

    public float CalculateVolume(BinaryData data)
    {
        var buffer = data.ToArray();
        var bytesRecorded = buffer.Length;
        int sum = 0;
        for (int i = 0; i < bytesRecorded; i += 2)
        {
            short sample = (short)(buffer[i + 1] << 8 | buffer[i]);
            sum += Math.Abs(sample);
        }
        return (float)sum / (bytesRecorded / 2) / 32768f;
    }
}
