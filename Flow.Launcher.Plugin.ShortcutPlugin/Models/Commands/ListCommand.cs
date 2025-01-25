using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using FuzzySharp;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class ListCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;
    private readonly List<ShortcutType> _shortcutTypes = Enum.GetValues<ShortcutType>().ToList();

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
               .WithHandler(GetShortcutListCommandHandler)
               .Build();
    }

    private List<Result> GetShortcutListCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var commandArguments = parsedQuery.CommandArguments;

        if (commandArguments.Count < 2)
        {
            return _shortcutsService.GetShortcutsList(parsedQuery.Arguments);
        }

        var shortcutType = commandArguments[1].ToLower();
        var matchingShortcutTypes = _shortcutTypes
                                    .Where(x => Fuzz.PartialRatio(x.ToString().ToLower(), shortcutType) > 90)
                                    .OrderByDescending(x => Fuzz.Ratio(x.ToString().ToLower(), shortcutType))
                                    .ToList();

        return matchingShortcutTypes is {Count: > 0}
            ? _shortcutsService.GetShortcutsList(parsedQuery.Arguments, matchingShortcutTypes[0])
            : _shortcutsService.GetShortcutsList(parsedQuery.Arguments);
    }
}