using System.Text;
using OpenAI.Chat;

namespace OpenAI.Samples;

file static class ThemeManager
{
    private static readonly List<string> Themes =
    [
        "boo",
        "dishes",
        "art",
        "needle",
        "tank",
        "police",
    ];

    public static string GetRandomTheme() => Themes[Random.Shared.Next(Themes.Count)];
}

public class FunctionCalling(ChatClient chatClient)
{
    public (string, string) TellJoke()
    {
        var getThemeTool = ChatTool.CreateFunctionTool(
            functionName: nameof(ThemeManager.GetRandomTheme),
            functionDescription: "Get a random theme for knock-knock jokes"
        );
        List<ChatMessage> messages =
        [
            new SystemChatMessage("""
                                  You are given a joke with the following setup:
                                  Knock, knock!
                                  Olive.
                                  Olive who?
                                  Olive you and I miss you!

                                  Pick a random theme and create a similar joke with a funny punchline.
                                  """),
            new UserChatMessage("Tell me a joke.")
        ];

        ChatCompletionOptions options = new()
        {
            Tools = { getThemeTool },
        };

        bool requiresAction;
        do
        {
            requiresAction = false;
            ChatCompletion completion = chatClient.CompleteChat(messages, options);

            switch (completion.FinishReason)
            {
                case ChatFinishReason.Stop:
                {
                    // Add the assistant message to the conversation history.
                    messages.Add(new AssistantChatMessage(completion));
                    break;
                }

                case ChatFinishReason.ToolCalls:
                {
                    // First, add the assistant message with tool calls to the conversation history.
                    messages.Add(new AssistantChatMessage(completion));

                    // Then, add a new tool message for each tool call that is resolved.
                    foreach (var toolCall in completion.ToolCalls)
                    {
                        switch (toolCall.FunctionName)
                        {
                            case nameof(ThemeManager.GetRandomTheme):
                            {
                                var toolResult = ThemeManager.GetRandomTheme();
                                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            }
                            default:
                            {
                                // Handle other unexpected calls.
                                throw new ApplicationException("Unexpected tool call.");
                            }
                        }
                    }

                    requiresAction = true;
                    break;
                }

                case ChatFinishReason.Length:
                    throw new ApplicationException(
                        "Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                case ChatFinishReason.ContentFilter:
                    throw new ApplicationException("Omitted content due to a content filter flag.");

                case ChatFinishReason.FunctionCall:
                    throw new ApplicationException("Deprecated in favor of tool calls.");

                default:
                    throw new ApplicationException(completion.FinishReason.ToString());
            }
        } while (requiresAction);

        var userMessages = new StringBuilder();
        var assistantMessages = new StringBuilder();
        foreach (var message in messages)
        {
            switch (message)
            {
                case UserChatMessage userMessage:
                    userMessages.AppendLine(userMessage.Content[0].Text);
                    break;

                case AssistantChatMessage assistantMessage when assistantMessage.Content.Count > 0:
                    assistantMessages.AppendLine(assistantMessage.Content[0].Text);
                    break;

                case ToolChatMessage:
                    // Do not print any tool messages; let the assistant summarize the tool results instead.
                    break;
            }
        }

        return (userMessages.ToString(), assistantMessages.ToString());
    }
}