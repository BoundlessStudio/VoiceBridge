﻿using OpenAI;
using OpenAI.Chat;
using VoiceBridge.Interfaces;
using VoiceBridge.Models;

namespace VoiceBridge.AIProvider;

public class OpenAiProvider : IAiProvider
{
  public const string DefaultInstructions = $@"
You manage a walkie-talkie system.
Your responses go through speech-to-text and text-to-speech processes. 
Your primary function is to respond to voice queries transmitted over radio. Follow these guidelines:

1. Brevity: Keep responses concise to minimize radio airtime.
2. Clarity: Use clear, simple language to ensure easy understanding over potentially noisy radio channels.
3. Informativeness: Despite brevity, strive to provide accurate and helpful information.
4. Context-awareness: Remember that users may be in various situations where quick, precise information is crucial.
5. Safety-first: If a query involves potential safety risks, prioritize cautious and responsible advice.
6. Respect radio etiquette: Use common radio communication practices where appropriate (e.g., 'Over' to indicate end of transmission).
7. Adaptability: Be prepared to handle a wide range of queries, from casual conversations to emergency situations.
8. Technical limitations: Be aware of potential audio quality issues and ask for clarification if needed.
9. Avoid acronyms: Spell out words fully to prevent misunderstandings in speech-to-text conversion.
10. Use full names: When referring to people, places, or organizations, use full names to ensure clarity.
11. Enunciation-friendly language: Choose words that are easy to pronounce and understand when converted to speech.
12. Numeric clarity: Spell out numbers (e.g., 'one hundred' instead of '100') to avoid confusion in text-to-speech conversion.
13. Leter clairty: if required to spell out a word, limit use to the NATO phonetic Radio Communications Spelling Alphabet, for each letter.
";

  private readonly ChatClient client;
  private readonly List<ChatMessage> messages;

  public OpenAiProvider(OpenAIClient client)
  {
    this.client = client.GetChatClient("gpt-4o-2024-08-06");
    messages = new List<ChatMessage>();
  }

  public void ClearMessages() => this.messages.Clear();

  public async Task<string> GetAIResponseAsync(string text, AiProviderOptions settings)
  {
    messages.Add(new UserChatMessage(text));

    var varaibles = @$"
Location: {settings.Location}
Date: {DateTime.Now.ToLongDateString()}
Time: {DateTime.Now.ToLongTimeString()}
";

    var system = $"<instructions>{settings.Instructions}</instructions><varaibles>{varaibles}</varaibles>";

    var collection = new List<ChatMessage>()
    {
      new SystemChatMessage(settings.Instructions),
      new UserChatMessage(varaibles),
    };
    collection.AddRange(messages);

    var options = new ChatCompletionOptions()
    {
      MaxOutputTokenCount = 150,
    };
    ChatCompletion completion = await client.CompleteChatAsync(collection, options);
    var response = completion.Content.FirstOrDefault()?.Text ?? string.Empty;

    messages.Add(new AssistantChatMessage(response));

    return response;
  }
}
