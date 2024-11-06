using System.Text;
using System.Text.Json;
using OpenAI.Chat;

namespace OpenAI.Samples;

public class PromptEngineering(ChatClient chatClient)
{
    public string Refine(string prompt, int maxIterations = 3)
    {
        Console.WriteLine("Original prompt: {0}", prompt);

        const string systemPrompt =
            """
            You are an expert in prompt engineering and large language models. 
            A good prompt should assign one or many roles to GPT, define a clear context and task, and clarify 
            expected output. You know and use many prompt techniques such as: Few-Shot Learning, Prompt Chaining, 
            Shadow Prompting, ... 
            I want you to be my personal prompt creator expert. Your name is now 'Promptor' and that is how I will 
            address you from now on. Promptor and GPT are separate and distinct entities. 
            You, Promptor, are responsible for creating good prompts for GPT.
            """;
        var currentPrompt = prompt;
        var questionAnswers = string.Empty;

        for (var i = 1; i <= maxIterations; i++)
        {
            Console.WriteLine("Iteration {0}", i);
            var reviews = Review(systemPrompt, currentPrompt);
            var questionsAnswers = Question(systemPrompt, currentPrompt, reviews, questionAnswers);
            currentPrompt = MakePrompt(systemPrompt, currentPrompt, reviews, questionsAnswers);

            Console.WriteLine();
            Console.WriteLine("New prompt: {0}", currentPrompt);
            Console.WriteLine();
            Console.WriteLine("Do you want to keep this prompt? (y/n)");
            var keep = Console.ReadLine();
            if (keep is "y" or "Y")
            {
                break;
            }
        }

        return currentPrompt;
    }

    private string Review(string initialPrompt, string currentPrompt)
    {
        var reviewPromptBuilder = new StringBuilder(initialPrompt);
        reviewPromptBuilder.AppendLine().AppendLine();
        reviewPromptBuilder.AppendLine($"This is my prompt: {currentPrompt}");
        reviewPromptBuilder.AppendLine("""
                                       Task: Provide a detailed, rigorous critique of my prompt.
                                       To do this, first start by giving my prompt a score from 0 to 5 (0 for poor, 
                                       5 for very optimal), and then write a short paragraph detailing improvements 
                                       that would make my prompt a perfect prompt with a score of 5.
                                       """);
        var review = CompleteChat(reviewPromptBuilder.ToString());
        Console.WriteLine(review);
        return review;
    }

    private string Question(string initialPrompt, string currentPrompt, string reviews, string questionsAnswers)
    {
        var questionPromptBuilder = new StringBuilder(initialPrompt);
        questionPromptBuilder.AppendLine().AppendLine();
        questionPromptBuilder.AppendLine($"This is my prompt: {currentPrompt}");
        questionPromptBuilder.AppendLine();
        questionPromptBuilder.AppendLine($"These are the reviews: {reviews}");
        questionPromptBuilder.AppendLine();
        questionPromptBuilder.AppendLine($"These are my answers to the questions: {questionsAnswers}");
        questionPromptBuilder.AppendLine(
            """
            Task: Compile a list of maximum 4 sort questions whose answers are indispensable for improving 
            my prompt (also give examples of answers in baskets.). 
            Output format: In JSON format. The output must be accepted by json.loads. The json format should 
            be like: {'Questions': ['Question 1','Question 2','Question 3','Question 4']}
            """);
        var questionJson = CompleteChat(questionPromptBuilder.ToString(), useJson: true);
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(questionJson));

        List<string?> questions = [];
        if (JsonDocument.TryParseValue(ref reader, out var questionsDocument))
        {
            questionsDocument.RootElement.GetProperty("Questions").EnumerateArray().ToList().ForEach(x => questions.Add(x.GetString()));
        }
        else
        {
            Console.WriteLine("Failed to parse questions from the model's response.");
        }

        var i = 1;
        var qnaBuilder = new StringBuilder(questionsAnswers);
        foreach (var question in questions)
        {
            Console.WriteLine($"Question {i}: {question}");
            var answer = Console.ReadLine();
            qnaBuilder.AppendLine($"Question: {question}");
            qnaBuilder.AppendLine($"Answer: {answer}");
            qnaBuilder.AppendLine();
            i++;
        }

        return qnaBuilder.ToString();
    }

    private string MakePrompt(string initialPrompt, string currentPrompt, string reviews, string questionsAnswers)
    {
        var promptBuilder = new StringBuilder(initialPrompt);
        promptBuilder.AppendLine().AppendLine();
        promptBuilder.AppendLine($"This is my prompt: {currentPrompt}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"This is the critical review of my prompt: {reviews}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine($"Some questions and answers for improving my prompt: {questionsAnswers}");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine(
            """
            Task: With all of this information, use all of your prompt engineering expertise to rewrite my 
            current prompt in the best possible way to create a perfect prompt for GPT with a score of 5. 
            All the information contained in the questions and answers must be included in the new prompt.
            Start the prompt by assigning one or many roles to GPT, defining the context, and the task.
            Output: It's very important that you only return the new prompt for GPT that you've created, 
            and nothing else.
            """);
        return CompleteChat(promptBuilder.ToString());
    }

    private string CompleteChat(string prompt, int temperature = 0, bool useJson = false)
    {
        ChatCompletion completion = chatClient.CompleteChat(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions
            {
                Temperature = temperature,
                ResponseFormat = useJson
                    ? ChatResponseFormat.CreateJsonObjectFormat()
                    : ChatResponseFormat.CreateTextFormat()
            });
        return completion.Content[0].Text;
    }
}