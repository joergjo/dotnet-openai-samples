using Azure.AI.OpenAI;
using OpenAI.Audio;

namespace OpenAI.Samples.Preview;

public static class Scenarios
{
    public static (string, string) TextToSpeech(AudioClient client, string text, string file)
    {
        var options = new SpeechGenerationOptions
        {
            ResponseFormat = GeneratedSpeechFormat.Mp3,
            SpeedRatio = 1.0f
        };
        BinaryData result = client.GenerateSpeech(text, GeneratedSpeechVoice.Alloy, options);
        using var fileStream = File.OpenWrite(file);
        fileStream.Write(result);
        return (text, $"Audio saved to {file}");
    }

    public static async Task<(string, string)> AssistZeldaPlayer(AzureOpenAIClient client, string deployment, string question)
    {
        var assistant = new ZeldaAssistant(client, deployment);
        return await assistant.PromptAsync(question);
    }
}