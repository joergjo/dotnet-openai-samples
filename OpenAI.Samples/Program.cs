// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Samples;
using static OpenAI.Samples.Terminal.Output;
using ScenarioResult = (string Prompt, string Response);
using StreamingScenarioResult = (string Prompt, System.Collections.Generic.IEnumerable<OpenAI.Chat.StreamingChatCompletionUpdate?> Updates);

// ReSharper disable JoinDeclarationAndInitializer

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var configuration = builder.Build();

var endpoint = configuration["OpenAI:Endpoint"];
var apiKey = configuration["OpenAI:ApiKey"];
var deployment = configuration["OpenAI:Deployment:Default"];
var visionDeployment = configuration["OpenAI:Deployment:Vision"];
var instructDeployment = configuration["OpenAI:Deployment:Instruct"];

if (endpoint is not { Length: > 0 } ||
    apiKey is not { Length: > 0 } ||
    deployment is not { Length: > 0 } ||
    visionDeployment is not { Length: > 0 } ||
    instructDeployment is not { Length: > 0 })
{
    Console.WriteLine(
        """
        Please provide OpenAI:Endpoint, OpenAI:ApiKey, OpenAI:Deployment:Default, OpenAI:Deployment:Instruct,
        and OpenAI:Deployment:Vision in appsettings.json, user secrets, environment variables, or command line 
        arguments.
        """);
    return;
}

var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
var chatClient = client.GetChatClient(deployment);
var instructClient = client.GetChatClient(instructDeployment);

ScenarioResult result;
StreamingScenarioResult streamingResult;

result = Scenarios.TellAJoke(chatClient);
Print(result.Prompt, result.Response);

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

chatClient = client.GetChatClient(visionDeployment);
result = Scenarios.SummarizePicture(chatClient, "../assets/General_Dynamic_F-16_USAF.jpg");
Print(result.Prompt, result.Response);
result = Scenarios.SummarizePicture(chatClient, new Uri("https://upload.wikimedia.org/wikipedia/commons/f/f8/General_Dynamic_F-16_USAF.jpg"));
Print(result.Prompt, result.Response);

var audioClient = client.GetAudioClient("whisper");
result = Scenarios.SpeechToText(audioClient, "../assets/openai.mp3");
Print(result.Prompt, result.Response);

result = Scenarios.ImproveUserPrompt(
    instructClient,
    "Give me a suggestion for family vacation in Scotland");
Print(result.Prompt, result.Response);
