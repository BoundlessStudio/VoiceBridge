namespace VoiceBridge.Models;

public class SpeechAvailableEventArgs : EventArgs
{
  public BinaryData Buffer { get; private set; }


  public static BinaryData Merge(List<BinaryData> collection)
  {
    using (MemoryStream ms = new MemoryStream())
    using (BinaryWriter bw = new BinaryWriter(ms))
    {
      // Write audio data
      foreach (var chunk in collection)
      {
        bw.Write(chunk.ToArray());
      }

      // Convert MemoryStream to BinaryData
      return BinaryData.FromBytes(ms.ToArray());
    }
  }


  public SpeechAvailableEventArgs(List<BinaryData> collection)
  {
    this.Buffer = Merge(collection);
  }
}
