// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using OpenAI.Samples.Chat;
using Spectre.Console;
using ScenarioResult = (string prompt, string response);

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .AddCommandLine(args);

var configuration = builder.Build();

var endpoint = configuration["OpenAI:Endpoint"];
var apiKey = configuration["OpenAI:ApiKey"];
var deployment = configuration["OpenAI:Deployment"];
var deploymentVision = configuration["OpenAI:DeploymentVision"];

if (endpoint is not { Length: > 0 } ||
    apiKey is not { Length: > 0 } ||
    deployment is not { Length: > 0 } ||
    deploymentVision is not { Length: > 0 })
{
    Console.WriteLine(
        "Please provide OpenAI:Endpoint, OpenAI:ApiKey, OpenAI:Deployment, and OpenAI:DeploymentVision in appsettings.json, user secrets, environment variables, or command line arguments.");
    return;
}

var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
var chatClient = client.GetChatClient(deployment);

// ReSharper disable once JoinDeclarationAndInitializer
ScenarioResult result;

result = Scenarios.TalkLikeAPirate(chatClient);
Show(result.prompt, result.response);

result = Scenarios.AssistJournalist(chatClient, "informal", 100, "blogpost", "The sky is blue", "The grass is green");
Show(result.prompt, result.response);

result = Scenarios.AssistJournalist(chatClient, "excited", 50, "news flash",
    "A book on ChatGPT has been published last week",
    "The title is Developing Apps with GPT-4 and ChatGPT", "The publisher is O'Reilly.");
Show(result.prompt, result.response);

Scenarios.SummarizeTranscript(chatClient, "./sampledata/brk255.txt");

chatClient = client.GetChatClient(deploymentVision);
result = Scenarios.SummarizePicture(chatClient, "./sampledata/F-16.jpg");
Show(result.prompt, result.response);
return;

static void Show(string prompt, string response)
{
    var rule = new Rule("[green]Prompt[/]");
    AnsiConsole.Write(rule);
    AnsiConsole.WriteLine();
    AnsiConsole.Markup("[green]{0}[/]", prompt);
    AnsiConsole.WriteLine();
    rule = new Rule("[blue]Response[/]");
    AnsiConsole.Write(rule);
    AnsiConsole.WriteLine();
    AnsiConsole.Markup("[blue]{0}[/]", response);
    AnsiConsole.WriteLine();
}