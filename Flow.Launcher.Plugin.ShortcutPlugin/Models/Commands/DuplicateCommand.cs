using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class DuplicateCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;

    public DuplicateCommand(IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
    }

    public Command Create()
    {
        return CreateDuplicateCommand();
    }

    private Command CreateDuplicateCommand()
    {
        var duplicateNewNameArgument = new ArgumentBuilder()
                                       .WithResponseInfo(("Enter new shortcut name",
                                           "How should your shortcut be named?"))
                                       .WithResponseSuccess(("Duplicate", "Your shortcut will be duplicated"))
                                       .WithHandler(DuplicateCommandHandler)
                                       .Build();

        var duplicateCurrentNameArgument = new ArgumentBuilder()
                                           .WithResponseInfo(("Enter shortcut name",
                                               "Which shortcut should be duplicated?"))
                                           .WithArguments(duplicateNewNameArgument)
                                           .Build();

        return new CommandBuilder()
               .WithKey("duplicate")
               .WithResponseInfo(("duplicate", "Duplicate shortcut"))
               .WithResponseFailure(("Failed to duplicate shortcut", "Something went wrong"))
               .WithArgument(duplicateCurrentNameArgument)
               .Build();
    }

    private List<Result> DuplicateCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;
        return _shortcutsService.DuplicateShortcut(arguments[1], arguments[2]);
    }
}