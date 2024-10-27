using OpenAI.Audio;

namespace OpenAI.Samples.Preview;

public static class Scenarios
{
    public static (string, string) TextToSpeech(AudioClient client, string text, string file)
    {
        var result = client.GenerateSpeech(text, GeneratedSpeechVoice.Onyx);
        using var fileStream = File.OpenWrite(file);
        result.Value.ToStream().CopyTo(fileStream);
        return (text, $"Audio saved to {file}");
    }
}