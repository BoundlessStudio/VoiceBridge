using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;
using VoiceBridge.AIProvider;
using VoiceBridge.App.Services;
using VoiceBridge.App.Views;
using VoiceBridge.Detectors;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;
using VoiceBridge.Voice;

namespace VoiceBridge.App;

public static class MauiProgram
{
  public static MauiApp CreateMauiApp()
  {
    var builder = MauiApp.CreateBuilder();

    builder
        .UseMauiApp<App>()
        //.UseMauiCommunityToolkit()
        .ConfigureFonts(fonts =>
        {
          fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
          fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });


    // Register services
    builder.Services.AddSingleton<IActivityDetector, VolumeActivityDetector>();
    builder.Services.AddSingleton<ISpeechToTextProvider, OpenAIWhisperProvider>();
    builder.Services.AddSingleton<ITextToSpeechProvider, OpenAIVoiceProvider>();
    builder.Services.AddSingleton<IAiProvider, OpenAiProvider>();
#if ANDROID
    builder.Services.AddSingleton<IAudioRecorder, VoiceBridge.App.Devices.AudioRecorder>();
    builder.Services.AddSingleton<IPlaybackDevice, VoiceBridge.App.Devices.AudioPlayer>();
#endif

    builder.Services.AddSingleton(sp =>
    {
      var settings = sp.GetRequiredService<ISettingsService>();
      var options = new AiProviderOptions()
      {
        AiCallSign = settings.AiCallSign,
        UserCallSign = settings.UserCallSign
      };
      return options;
    });
    builder.Services.AddSingleton(sp => {
      var settings = sp.GetRequiredService<ISettingsService>();
      var client = new OpenAIClient(new ApiKeyCredential(settings.OpenAiKey));
      return client;
    });
    
    builder.Services.AddSingleton<ISettingsService, SettingsService>();

    // Register pages
    builder.Services.AddTransient<MainPage>();
    builder.Services.AddTransient<BridgePage>();
    builder.Services.AddTransient<SettingsPage>();

    // Register view models
    builder.Services.AddTransient<MainViewModel>();
    builder.Services.AddTransient<BridgeViewModel>();
    builder.Services.AddTransient<SettingsViewModel>();

#if DEBUG
    builder.Logging.AddDebug();
#endif

    return builder.Build();
  }
}