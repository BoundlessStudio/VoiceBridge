using NAudio.Utils;
using NAudio.Wave;
using VoiceBridge.Interfaces;

namespace VoiceBridge.Devices;

public class AudioDeviceInput : IRecordingDevice, IDisposable
{
  public bool IsRecording { get; set; }

  public event EventHandler<RecordingCapturedEventArgs>? RecordingCaptured;

  private readonly WaveFormat waveFormat = new WaveFormat(16000, 16, 1);
  private readonly WaveInEvent inputDevice;
  private bool isRecording = false;

  private readonly List<byte> recording;
  private readonly IActivityDetector detector;

  public AudioDeviceInput(IActivityDetector detector)
  {
    recording = new List<byte>();
    this.detector = detector;
    inputDevice = new WaveInEvent
    {
      DeviceNumber = 0,
      WaveFormat = waveFormat
    };
    inputDevice.DataAvailable += WaveIn_DataAvailable;
  }

  private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
  {
    // if (this.audio.IsPlaying) return;

    var buffer = e.Buffer.Take(e.BytesRecorded);
    var data = BinaryData.FromBytes(buffer.ToArray());
    isRecording = detector.ActivityDetected(data);
    if (isRecording)
    {
      recording.AddRange(buffer);
    }
    else
    {
      var args = new RecordingCapturedEventArgs()
      {
        Data = BinaryData.FromBytes(recording.ToArray())
      };
      RecordingCaptured?.Invoke(this, args);
      recording.Clear();
    }
  }

  public async Task<Stream> WriteRecording(BinaryData data)
  {
    var speech = data.ToArray();
    using var stream = new MemoryStream();
    using var writer = new WaveFileWriter(new IgnoreDisposeStream(stream), waveFormat);
    await writer.WriteAsync(speech, 0, speech.Length);
    writer.Dispose();
    stream.Seek(0, SeekOrigin.Begin);
    return stream;
  }

  public void Start()
  {
    inputDevice.StartRecording();
  }

  public void Stop()
  {
    inputDevice.StopRecording();
  }

  public void Dispose() => inputDevice?.Dispose();

}
