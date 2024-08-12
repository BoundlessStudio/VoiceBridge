using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using VoiceBridge.AIProvider;
using VoiceBridge.Detectors;
using VoiceBridge.Devices;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;
using VoiceBridge.Voice;

namespace VoiceBridge;

public static class ServiceCollectionExtenstions
{
  public static IServiceCollection AddDefaultVoiceBridge(this IServiceCollection services)
  {
    services.AddOptionsWithValidateOnStart<VoiceBridgeOptions>("VoiceBridge");

    services.AddSingleton(sp => {
      var options = sp.GetRequiredService<IOptions<VoiceBridgeOptions>>();
      var client = new OpenAIClient(new ApiKeyCredential(options.Value.OpenAiKey));
      return client;
    });
    services.AddSingleton<IActivityDetector, VolumeActivityDetector>();
    services.AddSingleton<IRecordingDevice, AudioDeviceInput>();
    services.AddSingleton<ISpeechToTextProvider, OpenAIWhisperProvider>();
    services.AddSingleton<ITextToSpeechProvider, OpenAIVoiceProvider>();
    services.AddSingleton<IAiProvider, OpenAiProvider>();
    services.AddSingleton<IPlaybackDevice, AudioDeviceOuput>();
    services.AddSingleton<ISpeechPipeline, SpeechPipeline>();
    

    return services;
  }
}
