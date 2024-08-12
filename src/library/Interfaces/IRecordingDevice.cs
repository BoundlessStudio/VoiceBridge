namespace VoiceBridge.Interfaces;

public class RecordingCapturedEventArgs : EventArgs
{
    public BinaryData Data { get; set; }
}

public interface IRecordingDevice
{
  public void Start();
  public void Stop();

  public bool IsRecording { get; set; }

  public event EventHandler<RecordingCapturedEventArgs>? RecordingCaptured;

  Task<Stream> WriteRecording(BinaryData data);
}
