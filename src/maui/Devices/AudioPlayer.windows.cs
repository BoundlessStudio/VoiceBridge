
#if WINDOWS
using VoiceBridge.Interfaces;
using Windows.Media.Audio;
using Windows.Media.Render;
using Windows.Media;
using Windows.Storage.Streams;

namespace VoiceBridge.App.Devices;

public class AudioPlayer : IPlaybackDevice, IDisposable
{
    private AudioGraph audioGraph;
    private AudioDeviceOutputNode deviceOutputNode;
    private AudioFrameInputNode frameInputNode;
    private const int SampleRate = 24100;
    private const int ChannelCount = 1;

    public bool IsPlaying { get; private set; }

    public AudioPlayer()
    {
        var settings = new AudioGraphSettings(AudioRenderCategory.Media)
        {
            PrimaryRenderDevice = null,
            DesiredSamplesPerQuantum = SampleRate,
            QuantumSizeSelectionMode = QuantumSizeSelectionMode.SystemDefault
        };

        var result = AudioGraph.CreateAsync(settings).GetAwaiter().GetResult();
        if (result.Status != AudioGraphCreationStatus.Success)
        {
            throw new Exception("AudioGraph creation failed.");
        }

        audioGraph = result.Graph;

        var deviceOutputResult = audioGraph.CreateDeviceOutputNodeAsync().GetAwaiter().GetResult();
        if (deviceOutputResult.Status != AudioDeviceNodeCreationStatus.Success)
        {
            throw new Exception("Device output node creation failed.");
        }

        deviceOutputNode = deviceOutputResult.DeviceOutputNode;
        frameInputNode = audioGraph.CreateFrameInputNode();
        frameInputNode.AddOutgoingConnection(deviceOutputNode);
        audioGraph.Start();
    }

    public void Start()
    {
        frameInputNode.Start();
        IsPlaying = true;
    }

    public void Stop()
    {
        frameInputNode.Stop();
        IsPlaying = false;
    }

    public void Flush()
    {

    }

    public Task WriteAsync(BinaryData data)
    {
      this.Write(data);
      return Task.CompletedTask;
    }

     public void Write(BinaryData data)
    {
        var memory = data.ToArray();
        var frame = new AudioFrame((uint)memory.Length);

        using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
        {
            using (var reference = buffer.CreateReference())
            {
                IBufferByteAccess byteAccess = reference as IBufferByteAccess;
                if (byteAccess == null)
                {
                    throw new InvalidOperationException("Unable to get byte access for buffer.");
                }

                var writer = new DataWriter();
                writer.WriteBytes(memory);
                writer.StoreAsync().AsTask().Wait(); // Synchronous store
            }
        }

        frameInputNode.AddFrame(frame);
    }

    public void Dispose()
    {
        frameInputNode?.Dispose();
        deviceOutputNode?.Dispose();
        audioGraph?.Dispose();
    }
}
#endif
