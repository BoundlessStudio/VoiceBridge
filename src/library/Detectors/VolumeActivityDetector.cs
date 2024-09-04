using System.Diagnostics;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Detectors;

public class VolumeActivityDetector : IActivityDetector
{
  // TODO: Convert to Options
  const float VOLUME_THRESHOLD = 0.002f;
  const int SPEECH_PADDING_MS = 1000;

  private DateTime speechEndTime = DateTime.MinValue;
  private bool isRecording = false;


  public VolumeActivityDetector()
  {

  }

  public bool ActivityDetected(BinaryData data)
  {
    float volume = CalculateVolume(data);
    Debug.Write($"Volume {volume}:F4");

    if (volume > VOLUME_THRESHOLD)
    {
      if (isRecording)
      {
        speechEndTime = DateTime.Now.AddMilliseconds(SPEECH_PADDING_MS);
      }
      else
      {
        isRecording = true;
        Debug.Write($"Volume Threshold Trigger: {VOLUME_THRESHOLD}:F4");
      }
    }
    else if (isRecording && DateTime.Now > speechEndTime)
    {
      isRecording = false;
      Debug.Write("Activity Ended");
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
