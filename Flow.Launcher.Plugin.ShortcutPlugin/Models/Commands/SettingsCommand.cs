using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class SettingsCommand : ICommand
{
    private readonly IPluginManager _pluginManager;

    public SettingsCommand(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
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
            _pluginManager.API.OpenSettingDialog);
    }
}