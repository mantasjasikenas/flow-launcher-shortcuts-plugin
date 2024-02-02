using System;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ListCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;

    public ListCommand(IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
    }

    public Command Create()
    {
        return CreateListCommand();
    }

    private Command CreateListCommand()
    {
        return new CommandBuilder()
            .WithKey("list")
            .WithResponseInfo(("list", "List all shortcuts"))
            .WithResponseFailure(("Failed to show all shortcuts", "Something went wrong"))
            .WithResponseSuccess(("List", "List all shortcuts"))
            .WithMultipleValuesForSingleArgument()
            .WithHandler((_, arguments) => _shortcutsService.GetShortcuts(arguments))
            .Build();
    }
}