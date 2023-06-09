using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;
using Command = Flow.Launcher.Plugin.ShortcutPlugin.models.Command;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class CommandsService : ICommandsService
{
    public Dictionary<string, Func<string, string, List<Result>>> CommandsWithParams { get; }
    public Dictionary<string, Func<List<Result>>> CommandsWithoutParams { get; }


    private List<Helper> _helpers;

    private readonly string _pluginDirectory;

    private readonly PluginInitContext _context;

    private readonly IShortcutsService _shortcutsService;


    public CommandsService(PluginInitContext context, IShortcutsService shortcutsService)
    {
        _context = context;
        _shortcutsService = shortcutsService;
        _pluginDirectory = context.CurrentPluginMetadata.PluginDirectory;

        CommandsWithParams = new Dictionary<string, Func<string, string, List<Result>>>(StringComparer
            .InvariantCultureIgnoreCase);
        CommandsWithoutParams = new Dictionary<string, Func<List<Result>>>(StringComparer.InvariantCultureIgnoreCase);
        _helpers = LoadHelpersFile();

        Init();
    }

    public bool TryInvokeCommand(string query, out List<Result> results)
    {
        if (CommandsWithoutParams.TryGetValue(query, out var commandWithoutParams))
        {
            results = commandWithoutParams.Invoke();
            return true;
        }

        var queryParams = query.Split().Length;
        if (queryParams < 2)
        {
            results = ResultExtensions.InitializedResult();
            return false;
        }

        var command = Command.Parse(query);
        if (command is not null && CommandsWithParams.TryGetValue(command.Keyword, out var commandWithParams))
        {
            results = commandWithParams.Invoke(command.Id, command.Path);
            return true;
        }

        results = ResultExtensions.InitializedResult();
        return false;
    }

    private void Init()
    {
        LoadCommandsWithArgs();
        LoadCommandsWithoutArgs();
    }

    private void LoadCommandsWithoutArgs()
    {
        CommandsWithoutParams.Add("config", OpenConfigCommand);
        CommandsWithoutParams.Add("helpers", OpenHelpersCommand);
        CommandsWithoutParams.Add("reload", ReloadCommand);
        CommandsWithoutParams.Add("settings", OpenPluginSettingsCommand);
        CommandsWithoutParams.Add("help", HelpCommand);
        CommandsWithoutParams.Add("list", _shortcutsService.ListShortcuts);
        CommandsWithoutParams.Add("import", _shortcutsService.ImportShortcuts);
        CommandsWithoutParams.Add("export", _shortcutsService.ExportShortcuts);
    }

    private void LoadCommandsWithArgs()
    {
        CommandsWithParams.Add("add", _shortcutsService.AddShortcut);
        CommandsWithParams.Add("remove", _shortcutsService.RemoveShortcut);
        CommandsWithParams.Add("change", _shortcutsService.ChangeShortcutPath);
        CommandsWithParams.Add("path", _shortcutsService.GetShortcutPath);
    }


    private List<Result> OpenConfigCommand()
    {
        return OpenFileCommand(Constants.ShortcutsFileName,
            Resources.SettingsManager_OpenConfigCommand_Open_plugin_config);
    }

    private List<Result> OpenPluginSettingsCommand()
    {
        return ResultExtensions.SingleResult("Open Flow Launcher settings", "",
            () => { _context.API.OpenSettingDialog(); });
    }


    private List<Result> OpenHelpersCommand()
    {
        return OpenFileCommand(Constants.HelpersFileName,
            Resources.SettingsManager_OpenHelpersCommand_Open_plugin_helpers);
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
        return CommandsWithParams.Keys.Union(CommandsWithoutParams.Keys)
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
        var fullPath = Path.Combine(_pluginDirectory, Constants.HelpersFileName);
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