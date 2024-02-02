using System;
using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ReloadCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;
    private readonly ISettingsService _settingsService;
    private readonly IVariablesService _variablesService;

    public ReloadCommand(IShortcutsService shortcutsService, ISettingsService settingsService, IVariablesService variablesService)
    {
        _shortcutsService = shortcutsService;
        _settingsService = settingsService;
        _variablesService = variablesService;
    }

    public Command Create()
    {
        return CreateReloadCommand();
    }

    private Command CreateReloadCommand()
    {
        return new CommandBuilder()
            .WithKey("reload")
            .WithResponseInfo(("reload", "Reload plugin data"))
            .WithResponseSuccess(("Reload", "Reload plugin data"))
            .WithResponseFailure(("Failed to reload", "Something went wrong"))
            .WithHandler(ReloadCommandHandler)
            .Build();
    }

    private List<Result> ReloadCommandHandler(ActionContext context, List<string> arguments)
    {
        return ResultExtensions.SingleResult("Reload plugin data", "", () =>
        {
            _settingsService.Reload();
            _shortcutsService.Reload();
            _variablesService.Reload();
        });
    }
}