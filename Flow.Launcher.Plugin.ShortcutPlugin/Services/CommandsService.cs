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

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class CommandsService : ICommandsService
{
    private readonly Dictionary<string, Func<QueryCommand, List<Result>>> _commandsWithParams;
    private readonly Dictionary<string, Func<List<Result>>> _commandsWithoutParams;
    private List<Helper> _helpers;

    private readonly string _pluginDirectory;

    private readonly PluginInitContext _context;
    private readonly IShortcutsService _shortcutsService;
    private readonly ISettingsService _settingsService;


    public CommandsService(PluginInitContext context, IShortcutsService shortcutsService,
        ISettingsService settingsService)
    {
        _context = context;
        _shortcutsService = shortcutsService;
        _settingsService = settingsService;
        _pluginDirectory = context.CurrentPluginMetadata.PluginDirectory;

        _commandsWithParams = new Dictionary<string, Func<QueryCommand, List<Result>>>(StringComparer
            .InvariantCultureIgnoreCase);
        _commandsWithoutParams = new Dictionary<string, Func<List<Result>>>(StringComparer.InvariantCultureIgnoreCase);

        _helpers = LoadHelpersFile();

        Init();
    }

    public bool TryInvokeCommand(string query, out List<Result> results)
    {
        if (_commandsWithoutParams.TryGetValue(query, out var commandWithoutParams))
        {
            results = commandWithoutParams.Invoke();
            return true;
        }

        var command = QueryCommand.Parse(query);

        if (command is not null && _commandsWithParams.TryGetValue(command.Keyword, out var commandWithParams))
        {
            results = commandWithParams.Invoke(command);
            return true;
        }

        results = ResultExtensions.InitializedResult();
        return false;
    }

    private static List<string> SplitCommandLineArguments(string commandLine)
    {
        var arguments = new List<string>();

        var inQuotes = false;
        var isEscaped = false;
        var argStartIndex = 0;

        for (var i = 0; i < commandLine.Length; i++)
        {
            var currentChar = commandLine[i];

            switch (currentChar)
            {
                case '\\' when !isEscaped:
                    isEscaped = true;
                    continue;
                case '\"' when !isEscaped:
                    inQuotes = !inQuotes;
                    continue;
                case ' ' when !inQuotes:
                {
                    if (i > argStartIndex)
                    {
                        var argument = commandLine.Substring(argStartIndex, i - argStartIndex);

                        arguments.Add(argument);
                    }

                    argStartIndex = i + 1;
                    break;
                }
            }

            isEscaped = false;
        }

        if (commandLine.Length <= argStartIndex) return arguments;

        var lastArgument = commandLine[argStartIndex..];
        arguments.Add(lastArgument);

        arguments = arguments.Select(x => x.Replace("\"", "")).Where(x => !string.IsNullOrEmpty(x)).ToList();

        return arguments;
    }

    private void Init()
    {
        LoadCommandsWithArgs();
        LoadCommandsWithoutArgs();
    }

    private void LoadCommandsWithoutArgs()
    {
        _commandsWithoutParams.Add("config", OpenConfigCommand);
        _commandsWithoutParams.Add("helpers", OpenHelpersCommand);
        _commandsWithoutParams.Add("reload", ReloadCommand);
        _commandsWithoutParams.Add("settings", OpenPluginSettingsCommand);
        _commandsWithoutParams.Add("help", HelpCommand);
        _commandsWithoutParams.Add("list", _shortcutsService.ListShortcuts);
        _commandsWithoutParams.Add("import", _shortcutsService.ImportShortcuts);
        _commandsWithoutParams.Add("export", _shortcutsService.ExportShortcuts);
    }

    private void LoadCommandsWithArgs()
    {
        _commandsWithParams.Add("add", ParseAddShortcutCommand);
        _commandsWithParams.Add("remove", ParseRemoveShortcutCommand);
        _commandsWithParams.Add("change", ParseChangeShortcutCommand);
        _commandsWithParams.Add("path", ParseGetShortcutPathCommand);
    }

    private List<Result> ParseAddShortcutCommand(QueryCommand queryCommand)
    {
        var args = SplitCommandLineArguments(queryCommand.Args);


        if (args.Count < 2)
            return ResultExtensions.SingleResult("Invalid arguments", "Please provide shortcut name and path");

        var shortcutType = ShortcutType.Unknown;

        if (args.Count == 3)
            shortcutType = Enum.TryParse<ShortcutType>(args[2], true, out var type) ? type : ShortcutType.Unknown;

        return _shortcutsService.AddShortcut(args[0], args[1], shortcutType);
    }

    private List<Result> ParseRemoveShortcutCommand(QueryCommand queryCommand)
    {
        var args = SplitCommandLineArguments(queryCommand.Args);

        if (args.Count < 1)
            return ResultExtensions.SingleResult("Invalid arguments", "Please provide shortcut name");

        return _shortcutsService.RemoveShortcut(args[0]);
    }

    private List<Result> ParseGetShortcutPathCommand(QueryCommand queryCommand)
    {
        var args = SplitCommandLineArguments(queryCommand.Args);

        if (args.Count < 1)
            return ResultExtensions.SingleResult("Invalid arguments", "Please provide shortcut name");

        return _shortcutsService.GetShortcutPath(args[0]);
    }

    private List<Result> ParseChangeShortcutCommand(QueryCommand queryCommand)
    {
        var args = SplitCommandLineArguments(queryCommand.Args);

        if (args.Count < 2)
            return ResultExtensions.SingleResult("Invalid arguments", "Please provide shortcut name and path");

        return _shortcutsService.ChangeShortcutPath(args[0], args[1]);
    }


    private List<Result> OpenConfigCommand()
    {
        return OpenFileCommand(_settingsService.GetSetting(x => x.ShortcutsPath),
            Resources.SettingsManager_OpenConfigCommand_Open_plugin_config);
    }

    private List<Result> OpenPluginSettingsCommand()
    {
        return ResultExtensions.SingleResult("Open Flow Launcher settings", "",
            () => { _context.API.OpenSettingDialog(); });
    }


    private List<Result> OpenHelpersCommand()
    {
        var helpersPath = Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, Constants.HelpersFileName);

        return OpenFileCommand(helpersPath,
            Resources.SettingsManager_OpenHelpersCommand_Open_plugin_helpers);
    }


    private List<Result> OpenFileCommand(string path, string title)
    {
        return ResultExtensions.SingleResult(title, path,
            () =>
            {
                Clipboard.SetText(path);
                Cli.Wrap("powershell")
                   .WithArguments(path)
                   .ExecuteAsync();
            }
        );
    }

    public void ReloadData()
    {
        _settingsService.Reload();
        _shortcutsService.Reload();
        _helpers = LoadHelpersFile();
    }

    private List<Result> ReloadCommand()
    {
        return ResultExtensions.SingleResult(Resources.SettingsManager_ReloadCommand_Reload_plugin, action: ReloadData);
    }

    private List<Result> HelpCommand()
    {
        return _commandsWithParams.Keys.Union(_commandsWithoutParams.Keys)
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