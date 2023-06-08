using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class SettingsService : ISettingsService
{
    private readonly string _pluginDirectory;

    private readonly IShortcutsService _shortcutsService;
    private List<Helper> _helpers;


    public SettingsService(string pluginDirectory, IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
        _pluginDirectory = pluginDirectory;

        Settings = new Dictionary<string, Func<string, string, List<Result>>>(StringComparer
            .InvariantCultureIgnoreCase);
        Commands = new Dictionary<string, Func<List<Result>>>(StringComparer.InvariantCultureIgnoreCase);
        _helpers = LoadHelpersFile();

        Init();
    }

    public static string ShortcutsFileName => "config\\shortcuts.json";
    public static string HelpersFileName => "config\\helpers.json";
    public Dictionary<string, Func<string, string, List<Result>>> Settings { get; }
    public Dictionary<string, Func<List<Result>>> Commands { get; }

    private void Init()
    {
        //Settings commands
        Settings.Add("add", _shortcutsService.AddShortcut);
        Settings.Add("remove", _shortcutsService.RemoveShortcut);
        Settings.Add("change", _shortcutsService.ChangeShortcutPath);
        Settings.Add("path", _shortcutsService.GetShortcutPath);


        //Commands
        Commands.Add("config", OpenConfigCommand);
        Commands.Add("helpers", OpenHelpersCommand);
        Commands.Add("reload", ReloadCommand);
        Commands.Add("help", HelpCommand);
        Commands.Add("list", _shortcutsService.ListShortcuts);
        Commands.Add("import", _shortcutsService.ImportShortcuts);
        Commands.Add("export", _shortcutsService.ExportShortcuts);
    }

    private List<Result> OpenConfigCommand()
    {
        return OpenFileCommand(ShortcutsFileName, Resources.SettingsManager_OpenConfigCommand_Open_plugin_config);
    }


    private List<Result> OpenHelpersCommand()
    {
        return OpenFileCommand(HelpersFileName, Resources.SettingsManager_OpenHelpersCommand_Open_plugin_helpers);
    }


    private List<Result> OpenFileCommand(string filename, string title)
    {
        var pathConfig = Path.Combine(_pluginDirectory, filename);

        return ResultExtensions.SingleResult(title, pathConfig,
            () =>
            {
                Clipboard.SetText(pathConfig);
                Cli.Wrap("powershell")
                   .WithArguments(pathConfig)
                   .ExecuteAsync();
            }
        );
    }

    private List<Result> ReloadCommand()
    {
        return ResultExtensions.SingleResult(Resources.SettingsManager_ReloadCommand_Reload_plugin, action: () =>
        {
            _shortcutsService.Reload();
            _helpers = LoadHelpersFile();
        });
    }

    private List<Result> HelpCommand()
    {
        return Settings.Keys.Union(Commands.Keys)
                       .Select(x =>
                           new Result
                           {
                               Title = _helpers.Find(z => z.Keyword.Equals(x))?.Example,
                               SubTitle = _helpers.Find(z => z.Keyword.Equals(x))?.Description,
                               IcoPath = "images\\icon.png", Action = _ => true
                           })
                       .ToList();
    }


    private List<Helper> LoadHelpersFile()
    {
        var fullPath = Path.Combine(_pluginDirectory, HelpersFileName);
        if (!File.Exists(fullPath)) return new List<Helper>();

        try
        {
            return JsonSerializer.Deserialize<List<Helper>>(File.ReadAllText(fullPath));
        }
        catch (Exception)
        {
            return new List<Helper>();
        }
    }
}