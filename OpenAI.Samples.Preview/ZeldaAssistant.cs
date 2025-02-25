using System.Text;
using Azure.AI.OpenAI;
using OpenAI.Assistants;
using OpenAI.Files;

#pragma warning disable OPENAI001

namespace OpenAI.Samples.Preview;

public class ZeldaAssistant(AzureOpenAIClient openAIClient, string deployment)
{
    private const int MaxIterations = 20;

    public async Task<(string, string)> PromptAsync(string prompt)
    {
        // Upload the file to the OpenAI service
        var fileClient = openAIClient.GetOpenAIFileClient();
        OpenAIFile guide =
            await fileClient.UploadFileAsync("../assets/ExplorersGuide.pdf", FileUploadPurpose.Assistants);

        // Create the assistant
        var assistantClient = openAIClient.GetAssistantClient();
        var assistantCreateOptions = new AssistantCreationOptions
        {
            Name = "Zelda Expert",
            Instructions =
                """
                You're an expert on the video game Zelda, and you're going to answer my questions about the game 
                using the file I've given you.
                """,
            Tools = { new FileSearchToolDefinition() },
            ToolResources = new()
            {
                FileSearch = new()
                {
                    NewVectorStores = { new VectorStoreCreationHelper([guide.Id]) }
                }
            }
        };
        Assistant assistant = await assistantClient.CreateAssistantAsync(deployment, assistantCreateOptions);
        
        // Create the thread
        AssistantThread thread = await assistantClient.CreateThreadAsync();
        await assistantClient.CreateMessageAsync(thread.Id, MessageRole.User, [MessageContent.FromText(prompt)]);
        
        // Run the thread
        ThreadRun run = await assistantClient.CreateRunAsync(thread.Id, assistant.Id);

        // Wait for the run to complete
        var i = 0;
        do
        {
            run = await assistantClient.GetRunAsync(thread.Id, run.Id);
            Thread.Sleep(TimeSpan.FromSeconds(1));
        } while (!run.Status.IsTerminal || i++ < MaxIterations);

        // Abort if the run failed
        if (run.Status != RunStatus.Completed)
        {
            throw new ApplicationException($"Run failed with status {run.Status}");
        }

        // Collect messages from thread
        var messages = assistantClient.GetMessagesAsync(
            thread.Id, 
            new MessageCollectionOptions { Order  = MessageCollectionOrder.Ascending});

        var userMessages = new StringBuilder();
        var assistantMessages = new StringBuilder();
        await foreach (var message in messages)
        {
            var sb = message.Role switch
            {
                MessageRole.User => userMessages,
                MessageRole.Assistant => assistantMessages,
                _ => throw new ArgumentOutOfRangeException()
            };
            foreach (var content in message.Content)
            {
                if (content.Text is { Length: > 0 })
                {
                    sb.AppendLine(content.Text);
                }
            }
        }

        return (userMessages.ToString(), assistantMessages.ToString());
    }
}