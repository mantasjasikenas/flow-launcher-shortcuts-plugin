using System.Collections.Generic;
using System.IO;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

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
        var shortcutsPath = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        var variablesPath = _settingsService.GetSettingOrDefault(x => x.VariablesPath);

        return new List<Result>
        {
            CreateConfigResult("Open shortcuts config", shortcutsPath),
            CreateConfigResult("Open variables config", variablesPath)
        };
    }

    private Result CreateConfigResult(string title, string path)
    {
        return ResultExtensions.Result(title, path, () =>
            {
                if (!File.Exists(path))
                {
                    CreateConfigFile(path);
                }

                OpenConfig(path);
            },
            iconPath: File.Exists(path) ? path : null,
            autoCompleteText: path,
            previewFilePath: path,
            contextData: new FileShortcut()
        );
    }

    private static void OpenConfig(string path)
    {
        Cli.Wrap("powershell")
           .WithArguments(path)
           .ExecuteAsync();
    }

    private static void CreateConfigFile(string path)
    {
        var directory = Path.GetDirectoryName(path);

        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, "[]");
    }
}