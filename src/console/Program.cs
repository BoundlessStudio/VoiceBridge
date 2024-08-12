using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VoiceBridge;


await Host.CreateDefaultBuilder(args)
   .ConfigureAppConfiguration((hostingContext, config) =>
   {
     var env = hostingContext.HostingEnvironment;

     config.SetBasePath(env.ContentRootPath)
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables()
           .AddCommandLine(args);
   })
  .ConfigureServices((context, services) =>
  {
    services.AddDefaultVoiceBridge();
    services.AddHostedService<VoiceBackgroundService>();
  })
  .ConfigureLogging(logging =>
  {
    logging.ClearProviders();
    logging.AddConsole();
  })
  .RunConsoleAsync();