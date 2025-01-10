using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class SnippetsCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;

    public SnippetsCommand(IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
    }

    public Command Create()
    {
        return CreateSnippetCommand();
    }

    private Command CreateSnippetCommand()
    {
        return new CommandBuilder()
               .WithKey("snippets")
               .WithResponseInfo(("snippets", "List all snippets"))
               .WithResponseFailure(("Failed to show all snippets", "Something went wrong"))
               .WithResponseSuccess(("snippets", "List all snippets"))
               .WithMultipleValuesForSingleArgument()
               .WithHandler((_, arguments) =>
                   _shortcutsService.GetShortcutsList(arguments.Skip(1).ToList(), ShortcutType.Snippet))
               .Build();
    }
}