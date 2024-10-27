using Emgu.CV;
using OpenAI.Audio;
using OpenAI.Chat;

namespace OpenAI.Samples;

public static class Scenarios
{
    public static (string, string) TalkLikeAPirate(ChatClient chatClient)
    {
        const string userPrompt = "What's the best way to train a parrot?";
        ChatCompletion completion = chatClient.CompleteChat(
        [
            // System messages represent instructions or other guidance about how the assistant should behave
            new SystemChatMessage("You are a helpful assistant that talks like a pirate."),
            // User messages represent user input, whether historical or the most recen tinput
            new UserChatMessage("Hi, can you help me?"),
            // Assistant messages in a request represent conversation history for responses
            new AssistantChatMessage("Arrr! Of course, me hearty! What can I do for ye?"),
            new UserChatMessage(userPrompt),
        ]);
        return (userPrompt, completion.Content[0].Text);
    }

    public static (string, string) AssistJournalist(ChatClient chatClient, string tone, int length, string style,
        params string[] facts)
    {
        const string systemPrompt = """
                                    You are an assistant for journalists.
                                    Your task is to write articles, based on the FACTS that are given to you. 
                                    You should respect the instructions: the TONE, the LENGTH, and the STYLE
                                    """;

        var allFacts = string.Join(", ", facts);
        var userPrompt = $"""
                              FACTS: {allFacts} 
                              TONE: {tone}
                              LENGTH: {length} words
                              STYLE: {style}
                          """;

        ChatCompletion completion = chatClient.CompleteChat(
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt),
        ]);
        return (userPrompt, completion.Content[0].Text);
    }

    public static (string, IEnumerable<StreamingChatCompletionUpdate?>) SummarizeTranscript(ChatClient chatClient, string srcFile)
    {
        var transcript = File.ReadAllText(srcFile);
        var userPrompt = $"""
                          Please summarize the following video transcript:

                          {transcript}
                          """;
        var updates = chatClient.CompleteChatStreaming(
        [
            new UserChatMessage(userPrompt),
        ]);

        return ($"Please summarize the following video transcript... (see {srcFile})", updates);
    }

//     public static (string, string) SummarizeVideo(ChatClient chatClient, string srcFile, int start = 0, int count = 50)
//     {
//         var frame = new Mat();
//         var encodedFrames = new List<string>();
//         var frameCount = 0;
//         var last = start + count;
//         using var capture = new VideoCapture(srcFile);
//         while (capture.Read(frame) && frameCount < last)
//         {
//             if (frame.IsEmpty) break;
//             if (frameCount++ < start) continue;
//             
//             var buffer = CvInvoke.Imencode(".jpg", frame);
//             if (buffer is null) throw new Exception("Failed to encode frame");
//             var frameBase64 = $$"""{"image":"{{Convert.ToBase64String(buffer)}}", "resize":768""";
//             encodedFrames.Add(frameBase64);
//         }
//         
//         var userPrompt = $"""
//                           These are the first 50 frames of a video. Generate a two sentence summary.
//                           {string.Join(" ", encodedFrames)}
//                           """;
//
//         // Console.WriteLine("{0} characters", userPrompt.Length);
//         // return ("", "");
//         ChatCompletion completion = chatClient.CompleteChat(
//         [
//             new UserChatMessage(userPrompt),
//         ]);
//         return ($"These are the first 50 frames of a video. Generate a two sentence summary.", completion.Content[0].Text);
//     }
    
    public static (string, string) SummarizePicture(ChatClient chatClient, string srcFile)
    {
        const string userPrompt = "Please describe the following image.";
        using var imageStream = File.OpenRead(srcFile);
        var imageBytes = BinaryData.FromStream(imageStream);
        List<ChatMessage> messages = [
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart("Please describe the following image."),
                ChatMessageContentPart.CreateImagePart(imageBytes, "image/png"))
        ];
        ChatCompletion completion = chatClient.CompleteChat(
            messages, 
            new ChatCompletionOptions { MaxOutputTokenCount = 4096 });
        return (userPrompt, completion.Content[0].Text);
    }

    public static void PrepareVideoFrames(string srcFile, string dstDir = ".")
    {
        if (!Directory.Exists(dstDir)) throw new Exception("Destination directory does not exist.");
        
        var frameCount = 0;
        var frame = new Mat();
        using var capture = new VideoCapture(srcFile);
        while (capture.Read(frame))
        {
            if (frame.IsEmpty) break;
            
            var frameFile = Path.Combine(dstDir, $"frame{frameCount++}.jpg");
            var success = CvInvoke.Imwrite(frameFile, frame);
            if (!success) throw new Exception("Failed to write frame"); 
            Console.WriteLine($"Saved {frameFile}");
        }
    }
    
    public static (string, string) TextToSpeech(AudioClient client, string text, string file)
    {
        var result = client.GenerateSpeech(text, GeneratedSpeechVoice.Onyx);
        using var fileStream = File.OpenWrite(file);
        result.Value.ToStream().CopyTo(fileStream);
        return (text, $"Audio saved to {file}");
    }
}