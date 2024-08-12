using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VoiceBridge.Interfaces;

namespace VoiceBridge;

public class VoiceBackgroundService : BackgroundService
{
  private readonly ILogger logger;
  private readonly ISpeechPipeline pipeline;

  public VoiceBackgroundService(ILogger logger, ISpeechPipeline pipeline)
  {
    this.logger = logger;
    this.pipeline = pipeline;
  }

  private async Task CheckConsole(CancellationTokenSource cts)
  {
    await Task.Delay(50, cts.Token);

    if (!Console.KeyAvailable)
      return;

    var info = Console.ReadKey(intercept: true);
    switch (info.Key)
    {
      case ConsoleKey.Enter:
        cts.Cancel();
        break;
      case ConsoleKey.Spacebar:
        this.pipeline.Clear();
        break;
      default:
        break;
    }
  }

  protected override async Task ExecuteAsync(CancellationToken token)
  {
    logger.LogInformation("Listening for speech.");
    logger.LogInformation("Press [Enter] to exit.");
    logger.LogInformation("Press [Space] to clear messages.");

    var cts = CancellationTokenSource.CreateLinkedTokenSource(token);

    this.pipeline.Start();

    while (!cts.Token.IsCancellationRequested)
    {
      await CheckConsole(cts);
    }

    this.pipeline.Stop();
  }

}
