
using System;
using System.Collections.Generic;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ConfigCommand : ICommand
{
    private readonly ISettingsService _settingsService;

    public ConfigCommand(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public models.Command Create()
    {
        return CreateConfigCommand();
    }

    private models.Command CreateConfigCommand()
    {
        return new CommandBuilder()
            .WithKey("config")
            .WithResponseInfo(("config", "Configure plugin"))
            .WithResponseFailure(("Failed to open config", "Something went wrong"))
            .WithHandler(ConfigCommandHandler)
            .Build();
    }

    private List<Result> ConfigCommandHandler(ActionContext context, List<string> arguments)
    {
        var shortcutsPath = _settingsService.GetSetting(x => x.ShortcutsPath);
        var variablesPath = _settingsService.GetSetting(x => x.VariablesPath);

        return new List<Result>
        {
            ResultExtensions.Result("Open shortcuts config", shortcutsPath, () =>
            {
                Cli.Wrap("powershell")
                    .WithArguments(shortcutsPath)
                    .ExecuteAsync();
            }),
            ResultExtensions.Result("Open variables config", variablesPath, () =>
            {
                Cli.Wrap("powershell")
                    .WithArguments(variablesPath)
                    .ExecuteAsync();
            })
        };
    }
}