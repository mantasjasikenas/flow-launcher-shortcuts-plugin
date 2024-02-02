using System;
using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class SettingsCommand : ICommand
{
    private readonly PluginInitContext _context;

    public SettingsCommand(PluginInitContext context)
    {
        _context = context;
    }

    public Command Create()
    {
        return CreateSettingsCommand();
    }

    private Command CreateSettingsCommand()
    {
        return new CommandBuilder()
            .WithKey("settings")
            .WithResponseInfo(("settings", "Open settings"))
            .WithResponseFailure(("Failed to open settings", "Something went wrong"))
            .WithHandler(SettingsCommandHandler)
            .Build();
    }

    private List<Result> SettingsCommandHandler(ActionContext context, List<string> arguments)
    {
        return ResultExtensions.SingleResult("Open Flow Launcher settings", "",
            _context.API.OpenSettingDialog);
    }
}