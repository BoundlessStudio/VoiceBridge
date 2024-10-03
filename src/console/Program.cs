using OpenAI.RealtimeConversation;

//var instructions = $@"
//You manage a walkie-talkie system and your Call Sign is Cohesive.
//Your responses go through speech-to-text and text-to-speech processes. 
//Your primary function is to respond to voice queries transmitted over radio. Follow these guidelines:

//1. Brevity: Keep responses concise to minimize radio airtime.
//2. Clarity: Use clear, simple language to ensure easy understanding over potentially noisy radio channels.
//3. Informativeness: Despite brevity, strive to provide accurate and helpful information.
//4. Context-awareness: Remember that users may be in various situations where quick, precise information is crucial.
//5. Safety-first: If a query involves potential safety risks, prioritize cautious and responsible advice.
//6. Respect radio etiquette: Use common radio communication practices where appropriate (e.g., 'Over' to indicate end of transmission).
//7. Adaptability: Be prepared to handle a wide range of queries, from casual conversations to emergency situations.
//8. Technical limitations: Be aware of potential audio quality issues and ask for clarification if needed.
//9. Avoid acronyms: Spell out words fully to prevent misunderstandings in speech-to-text conversion.
//10. Use full names: When referring to people, places, or organizations, use full names to ensure clarity.
//11. Enunciation-friendly language: Choose words that are easy to pronounce and understand when converted to speech.
//12. Numeric clarity: Spell out numbers (e.g., 'one hundred' instead of '100') to avoid confusion in text-to-speech conversion.
//13. Leter clairty: if required to spell out a word, limit use to the NATO phonetic Radio Communications Spelling Alphabet, for each letter.

//Remember, your responses transmitted over radio, so focus on delivering the most important information efficiently.
//";

var client = new OpenAI.OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
var conversation = client.GetRealtimeConversationClient("gpt-4o-realtime-preview-2024-10-01");
using RealtimeConversationSession session = await conversation.StartConversationSessionAsync();

//var turnDetection = ConversationTurnDetectionOptions.CreateServerVoiceActivityTurnDetectionOptions(0.05f, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000));
//var options = new ConversationSessionOptions()
//{
//  InputAudioFormat = ConversationAudioFormat.Pcm16,
//  OutputAudioFormat = ConversationAudioFormat.Pcm16,
//  TurnDetectionOptions = turnDetection,
//  Voice = ConversationVoice.Alloy,
//  Instructions = instructions,
//  MaxResponseOutputTokens = 300,
//};
//await session.ConfigureSessionAsync(options);

var instructions = "You are a cheerful assistant that talks like a pirate. Always inform the user when you are about to call a tool. Prefer to call tools whenever applicable.";

// Session options control connection-wide behavior shared across all conversations,
// including audio input format and voice activity detection settings.
ConversationSessionOptions sessionOptions = new()
{
  Instructions = instructions,
  Voice = ConversationVoice.Alloy,
  Tools = { CreateSampleWeatherTool() },
  InputAudioFormat = ConversationAudioFormat.G711Alaw,
  OutputAudioFormat = ConversationAudioFormat.Pcm16,
  InputTranscriptionOptions = new()
  {
    Model = ConversationTranscriptionModel.Whisper1,
  },
};

await session.ConfigureSessionAsync(sessionOptions);

// Conversation history or text input are provided by adding messages to the conversation.
// Adding a message will not automatically begin a response turn.
await session.AddItemAsync(ConversationItem.CreateUserMessage(["I'm trying to decide what to wear on my trip."]));

string inputAudioPath = "input.wav";
using Stream inputAudioStream = File.OpenRead(inputAudioPath);
_ = session.SendAudioAsync(inputAudioStream);

string outputAudioPath = "output.raw";
using Stream outputAudioStream = File.OpenWrite(outputAudioPath);

await foreach (ConversationUpdate update in session.ReceiveUpdatesAsync())
{
  if (update is ConversationSessionStartedUpdate sessionStartedUpdate)
  {
    Console.WriteLine($"<<< Session started. ID: {sessionStartedUpdate.SessionId}");
    Console.WriteLine();
  }

  if (update is ConversationInputSpeechStartedUpdate speechStartedUpdate)
  {
    Console.WriteLine(
        $"  -- Voice activity detection started at {speechStartedUpdate.AudioStartMs} ms");
  }

  if (update is ConversationInputSpeechFinishedUpdate speechFinishedUpdate)
  {
    Console.WriteLine(
        $"  -- Voice activity detection ended at {speechFinishedUpdate.AudioEndMs} ms");
  }

  // Item started updates notify that the model generation process will insert a new item into
  // the conversation and begin streaming its content via content updates.
  if (update is ConversationItemStartedUpdate itemStartedUpdate)
  {
    Console.WriteLine($"  -- Begin streaming of new item");
    if (!string.IsNullOrEmpty(itemStartedUpdate.FunctionName))
    {
      Console.Write($"    {itemStartedUpdate.FunctionName}: ");
    }
  }

  // Audio transcript delta updates contain the incremental text matching the generated
  // output audio.
  if (update is ConversationOutputTranscriptionDeltaUpdate outputTranscriptDeltaUpdate)
  {
    Console.Write(outputTranscriptDeltaUpdate.Delta);
  }

  // Audio delta updates contain the incremental binary audio data of the generated output
  // audio, matching the output audio format configured for the session.
  if (update is ConversationAudioDeltaUpdate audioDeltaUpdate)
  {
    outputAudioStream.Write(audioDeltaUpdate.Delta?.ToArray() ?? []);
  }

  if (update is ConversationFunctionCallArgumentsDeltaUpdate argumentsDeltaUpdate)
  {
    Console.Write(argumentsDeltaUpdate.Delta);
  }

  // Item finished updates arrive when all streamed data for an item has arrived and the
  // accumulated results are available. In the case of function calls, this is the point
  // where all arguments are expected to be present.
  if (update is ConversationItemFinishedUpdate itemFinishedUpdate)
  {
    Console.WriteLine();
    Console.WriteLine($"  -- Item streaming finished, response_id={itemFinishedUpdate.ResponseId}");

    if (itemFinishedUpdate.FunctionCallId is not null)
    {
      Console.WriteLine($"    + Responding to tool invoked by item: {itemFinishedUpdate.FunctionName}");
      ConversationItem functionOutputItem = ConversationItem.CreateFunctionCallOutput(
          callId: itemFinishedUpdate.FunctionCallId,
          output: "24 degrees and sunny"
      );
      await session.AddItemAsync(functionOutputItem);
    }
    else if (itemFinishedUpdate.MessageContentParts?.Count > 0)
    {
      Console.Write($"    + [{itemFinishedUpdate.MessageRole}]: ");
      foreach (ConversationContentPart contentPart in itemFinishedUpdate.MessageContentParts)
      {
        Console.Write(contentPart.AudioTranscriptValue);
      }
      Console.WriteLine();
    }
  }

  if (update is ConversationInputTranscriptionFinishedUpdate transcriptionCompletedUpdate)
  {
    Console.WriteLine();
    Console.WriteLine($"  -- User audio transcript: {transcriptionCompletedUpdate.Transcript}");
    Console.WriteLine();
  }

  if (update is ConversationResponseFinishedUpdate turnFinishedUpdate)
  {
    Console.WriteLine($"  -- Model turn generation finished. Status: {turnFinishedUpdate.Status}");

    // Here, if we processed tool calls in the course of the model turn, we finish the
    // client turn to resume model generation. The next model turn will reflect the tool
    // responses that were already provided.
    if (turnFinishedUpdate.CreatedItems.Any(item => item.FunctionName?.Length > 0))
    {
      Console.WriteLine($"  -- Ending client turn for pending tool responses");
      await session.StartResponseTurnAsync();
    }
    else
    {
      break;
    }
  }

  if (update is ConversationErrorUpdate errorUpdate)
  {
    Console.WriteLine();
    Console.WriteLine($"ERROR: {errorUpdate.ErrorMessage}");
    break;
  }
}

Console.WriteLine($"Raw output audio written to {outputAudioPath}: {outputAudioStream.Length} bytes");
Console.WriteLine();

ConversationFunctionTool CreateSampleWeatherTool()
{
  return new ConversationFunctionTool()
  {
    Name = "get_weather_for_location",
    Description = "gets the weather for a location",
    Parameters = BinaryData.FromString("""
    {
      "type": "object",
      "properties": {
        "location": {
          "type": "string",
          "description": "The city and province and country, e.g. London, ON, CA."
        },
        "unit": {
          "type": "string",
          "enum": ["c","f"],
          "description": "unit of tempature default to Celsius (c)"
        }
      },
      "required": ["location","unit"]
    }
    """)
  };
}