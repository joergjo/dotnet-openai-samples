using System.ClientModel;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Configuration;
using OpenAI;
using static OpenAI.Samples.Terminal.Output;
using ScenarioResult = (string Prompt, string Response);
using StreamingScenarioResult = (string Prompt, System.Collections.Generic.IEnumerable<OpenAI.Chat.StreamingChatCompletionUpdate?> Updates);


var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var configuration = builder.Build();

var modelAlias = configuration["FoundryLocal:ModelAlias"] ?? "qwen2.5-1.5b";
var manager = await FoundryLocalManager.StartModelAsync(aliasOrModelId: modelAlias);

var model = await manager.GetModelInfoAsync(aliasOrModelId: modelAlias);
var key = new ApiKeyCredential(manager.ApiKey);
var client = new OpenAIClient(key, new OpenAIClientOptions
{
    Endpoint = manager.Endpoint
});

var chatClient = client.GetChatClient(model?.ModelId);

ScenarioResult result;
StreamingScenarioResult streamingResult;

Info($"Using model: {model?.ModelId} ({model?.Alias})");

result = Scenarios.TalkLikeAPirate(chatClient);
Print(result.Prompt, result.Response);

result = Scenarios.AssistJournalist(chatClient, "informal", 100, "blogpost", 
    "The sky is blue", "The grass is green");
Print(result.Prompt, result.Response);

result = Scenarios.AssistJournalist(chatClient, "excited", 50, "news flash",
    "A book on ChatGPT has been published last week",
    "The title is Developing Apps with GPT-4 and ChatGPT", "The publisher is O'Reilly.");
Print(result.Prompt, result.Response);

streamingResult = Scenarios.SummarizeTranscript(chatClient, "../assets/brk255.txt");
Stream(streamingResult.Prompt, streamingResult.Updates);

