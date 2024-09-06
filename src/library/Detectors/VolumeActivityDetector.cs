using System.Diagnostics;
using VoiceBridge.Models;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Detectors;

public class VolumeActivityDetector : IActivityDetector
{
  const int SPEECH_PADDING_MS = 1000;
  private DateTime speechEndTime = DateTime.MinValue;
  private bool isRecording = false;


  public VolumeActivityDetector()
  {

  }

  public bool ActivityDetected(BinaryData data, ActivityDetectorOptions options)
  {
    float volume = CalculateVolume(data);
    if (volume > options.VolumeThreshold)
    {
      if (isRecording)
      {
        speechEndTime = DateTime.Now.AddMilliseconds(SPEECH_PADDING_MS);
      }
      else
      {
        isRecording = true;
      }
    }
    else if (isRecording && DateTime.Now > speechEndTime)
    {
      isRecording = false;
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
    var volume = (float)sum / (bytesRecorded / 2) / 32768f;
    // Debug.Write($"Volume {volume}");
    return volume;
  }
}
