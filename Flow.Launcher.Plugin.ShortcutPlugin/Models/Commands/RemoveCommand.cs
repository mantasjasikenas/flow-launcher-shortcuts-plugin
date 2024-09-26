using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class RemoveCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;

    public RemoveCommand(IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
    }

    public Command Create()
    {
        return CreateRemoveCommand();
    }

    private Command CreateRemoveCommand()
    {
        var shortcutNameArgument = new ArgumentBuilder()
                                   .WithResponseInfo(("Enter shortcut name", "Which shortcut should be removed?"))
                                   .WithResponseSuccess(("Remove", "Your shortcut will be removed from the list"))
                                   .WithHandler(RemoveCommandHandler)
                                   .Build();

        return new CommandBuilder()
               .WithKey("remove")
               .WithResponseInfo(("remove", "Remove shortcuts from the list"))
               .WithResponseFailure(("Enter shortcut name", "Which shortcut should be removed?"))
               .WithArgument(shortcutNameArgument)
               .Build();
    }

    private List<Result> RemoveCommandHandler(ActionContext context, List<string> arguments)
    {
        return _shortcutsService.RemoveShortcut(arguments[1]);
    }
}