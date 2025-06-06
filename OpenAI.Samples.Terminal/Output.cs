using OpenAI.Chat;
using Spectre.Console;

namespace OpenAI.Samples.Terminal;

public static class Output
{
    private static void UserPrompt(string prompt)
    {
        var rule = new Rule("[green]User Prompt[/]");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[green]{0}[/]", prompt);
        AnsiConsole.WriteLine();
    }

    public static void Stream(string prompt, IEnumerable<StreamingChatCompletionUpdate?> updates)
    {
        UserPrompt(prompt);
        var rule = new Rule("[blue]Response[/]");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
        foreach (var update in updates)
        {
            if (update is null) continue;
            foreach (var message in update.ContentUpdate)
            {
                AnsiConsole.Markup("[blue]{0}[/]", message.Text);
            }
        }
        AnsiConsole.WriteLine();
    }

    public static void Print(string prompt, string response)
    {
        UserPrompt(prompt);
        var rule = new Rule("[blue]Response[/]");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[blue]{0}[/]", response);
        AnsiConsole.WriteLine();
    }
    
    public static void Info(string message)
    {
        AnsiConsole.MarkupLine("[yellow]{0}[/]", message);
    }
}