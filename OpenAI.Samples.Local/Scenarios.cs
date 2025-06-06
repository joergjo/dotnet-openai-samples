using OpenAI.Chat;

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

    public static (string, IEnumerable<StreamingChatCompletionUpdate?>) SummarizeTranscript(ChatClient chatClient, string source)
    {
        var transcript = File.ReadAllText(source);
        var userPrompt = $"""
                          Please summarize the following video transcript:

                          {transcript}
                          """;
        var updates = chatClient.CompleteChatStreaming(
        [
            new UserChatMessage(userPrompt),
        ]);

        return ($"Please summarize the following video transcript... (see {source})", updates);
    }
}