using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VoiceBridge;

public class VoiceBackgroundService : BackgroundService
{
  private readonly ILogger logger;

  public VoiceBackgroundService(ILogger logger)
  {
    this.logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken token)
  {
    logger.LogInformation("Listening for speech.");

    var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

    //this.pipeline.Start();

    while (!cts.Token.IsCancellationRequested)
    {
      await Task.Delay(100, cts.Token);
    }

    //this.pipeline.Stop();
  }

}
