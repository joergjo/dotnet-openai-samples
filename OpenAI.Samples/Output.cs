using OpenAI.Chat;
using Spectre.Console;

namespace OpenAI.Samples;

public static class Output
{
    public static void UserPrompt(string prompt)
    {
        var rule = new Rule("[green]User Prompt[/]");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[green]{0}[/]", prompt);
        AnsiConsole.WriteLine();
    }
    
    public static void StreamResponse(IEnumerable<StreamingChatCompletionUpdate?> updates)
    {
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
}