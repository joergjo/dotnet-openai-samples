using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Samples.Preview;
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

var endpoint = configuration["OpenAI:Endpoint"];
var apiKey = configuration["OpenAI:ApiKey"];
var deployment = configuration["OpenAI:Deployment:Default"];
var deploymentVision = configuration["OpenAI:Deployment:Vision"];
var deploymentTts = configuration["OpenAI:Deployment:Tts"];

if (endpoint is not { Length: > 0 } ||
    apiKey is not { Length: > 0 } ||
    deployment is not { Length: > 0 } ||
    deploymentVision is not { Length: > 0 } ||
    deploymentTts is not { Length: > 0 })
{
    Console.WriteLine(
        """
        Please provide OpenAI:Endpoint, OpenAI:ApiKey, OpenAI:Deployment:Default, OpenAI:Deployment:Vision, 
        and OpenAI:Deployment:Tts in appsettings.json, user secrets, environment variables, or command line arguments.
        """);
    return;
}

var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
var audioClient = client.GetAudioClient(deploymentTts);

ScenarioResult result;

const string text = """
                    OpenAI is an American artificial intelligence (AI) research organization founded in December 2015 
                    and headquartered in San Francisco, California. Its mission is to develop "safe and beneficial" 
                    artificial general intelligence (AGI), which it defines as "highly autonomous systems that 
                    outperform humans at most economically valuable work". As a leading organization in the ongoing 
                    AI boom, OpenAI is known for the GPT family of large language models, the DALL-E series of 
                    text-to-image models, and a text-to-video model named Sora.
                    """;
result = Scenarios.TextToSpeech(audioClient, text, "./openai.mp3");
Print(result.Prompt, result.Response);