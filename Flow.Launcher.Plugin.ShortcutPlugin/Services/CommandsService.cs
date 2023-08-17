using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class CommandsService : ICommandsService
{
    private readonly Dictionary<string, Func<List<Result>>> _commandsWithoutParams;
    private readonly Dictionary<string, Func<models.Query, List<Result>>> _commandsWithParams;

    private readonly PluginInitContext _context;

    private readonly IHelpersRepository _helpersRepository;
    private readonly ISettingsService _settingsService;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IShortcutsService _shortcutsService;
    private readonly IVariablesService _variablesService;
    private readonly ICommandsRepository _commandsRepository;


    public CommandsService(PluginInitContext context,
        IShortcutsService shortcutsService,
        IShortcutsRepository shortcutsRepository,
        ISettingsService settingsService,
        IHelpersRepository helpersRepository,
        IVariablesService variablesService,
        ICommandsRepository commandsRepository
    )
    {
        _context = context;
        _shortcutsService = shortcutsService;
        _shortcutsRepository = shortcutsRepository;
        _settingsService = settingsService;
        _helpersRepository = helpersRepository;
        _variablesService = variablesService;
        _commandsRepository = commandsRepository;

        _commandsWithParams = new Dictionary<string, Func<models.Query, List<Result>>>(StringComparer
            .InvariantCultureIgnoreCase);
        _commandsWithoutParams = new Dictionary<string, Func<List<Result>>>(StringComparer.InvariantCultureIgnoreCase);

        Init();
    }

    public List<Result> ResolveCommand(List<string> arguments)
    {
        return _commandsRepository.ResolveCommand(arguments);
    }

    public void ReloadPluginData()
    {
        _settingsService.Reload();
        _shortcutsService.Reload();
        _variablesService.Reload();
        _helpersRepository.Reload();
    }

    public bool TryInvokeCommand(string query, out List<Result> results)
    {
        // <--Query is shortcut-->
        if (TryInvokeShortcut(query, out var shortcutResult))
        {
            results = shortcutResult;
            return true;
        }

        // <--Command without params-->
        if (_commandsWithoutParams.TryGetValue(query, out var commandWithoutParams))
        {
            results = commandWithoutParams.Invoke();
            return true;
        }

        // <--Command with params-->
        var command = models.Query.Parse(query);
        if (command is not null && _commandsWithParams.TryGetValue(command.Keyword, out var commandWithParams))
        {
            results = commandWithParams.Invoke(command);
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
        _commandsWithoutParams.Add("config", OpenConfigCommand);
        _commandsWithoutParams.Add("helpers", OpenHelpersCommand);
        _commandsWithoutParams.Add("reload", ReloadCommand);
        _commandsWithoutParams.Add("settings", OpenPluginSettingsCommand);
        _commandsWithoutParams.Add("help", HelpCommand);
        _commandsWithoutParams.Add("list", _shortcutsService.GetShortcuts);
        _commandsWithoutParams.Add("import", _shortcutsService.ImportShortcuts);
        _commandsWithoutParams.Add("export", _shortcutsService.ExportShortcuts);
    }

    private void LoadCommandsWithArgs()
    {
        _commandsWithParams.Add("add", ParseAddShortcutCommand);
        _commandsWithParams.Add("var", ParseVariableCommand);
        _commandsWithParams.Add("remove", ParseRemoveShortcutCommand);
        _commandsWithParams.Add("path", ParseGetShortcutPathCommand);
        _commandsWithParams.Add("keyword", ParsePluginKeywordCommand);
        _commandsWithParams.Add("duplicate", ParseDuplicateShortcutCommand);
    }

    private bool TryInvokeShortcut(string shortcut, out List<Result> results)
    {
        // <--Query is shortcut-->
        if (_shortcutsRepository.GetShortcut(shortcut) is not null)
        {
            results = _shortcutsService.OpenShortcut(shortcut);
            return true;
        }

        results = null;
        return false;
    }


    private List<Result> ParsePluginKeywordCommand(models.Query query)
    {
        var args = CommandLineExtensions.SplitArguments(query.Args);

        return args.Count switch
        {
            0 => ResultExtensions.SingleResult("Plugin keyword",
                $"{_context.CurrentPluginMetadata.ActionKeyword}"),

            1 => ResultExtensions.SingleResult($"Set plugin keyword to '{args[0]}'",
                "",
                () =>
                {
                    var newKeyword = args[0];
                    var oldKeyword = _context.CurrentPluginMetadata.ActionKeyword;

                    _context.API.AddActionKeyword(_context.CurrentPluginMetadata.ID, newKeyword);
                    _context.CurrentPluginMetadata.ActionKeyword = newKeyword;

                    if (newKeyword.Equals(oldKeyword, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }

                    _context.API.RemoveActionKeyword(_context.CurrentPluginMetadata.ID, oldKeyword);
                }),

            _ => ResultExtensions.SingleResult("Invalid arguments", "Please provide only one argument - new keyword.")
        };
    }

    private List<Result> ParseDuplicateShortcutCommand(models.Query query)
    {
        var args = CommandLineExtensions.SplitArguments(query.Args);

        return args.Count switch
        {
            0 or 1 => ResultExtensions.SingleResult("Duplicate shortcut",
                "Please provide shortcut name and new shortcut name"),

            2 => _shortcutsService.DuplicateShortcut(args[0], args[1]),

            _ => ResultExtensions.SingleResult("Invalid arguments",
                "Please provide two arguments - existing shortcut and new name.")
        };
    }

    private List<Result> ParseVariableCommand(models.Query query)
    {
        var args = CommandLineExtensions.SplitArguments(query.Args);

        return args.Count switch
        {
            0 => _variablesService.GetVariables(),

            1 => _variablesService.GetVariable(args[0]),

            2 => _variablesService.AddVariable(args[0], args[1]),

            _ => ResultExtensions.SingleResult("Invalid arguments",
                "Please provide variable name and value or only variable name.")
        };
    }

    private List<Result> ParseAddShortcutCommand(models.Query query)
    {
        var args = CommandLineExtensions.SplitArguments(query.Args);

        // no args
        if (args.Count == 0)
        {
            return ResultExtensions.SingleResult("Add shortcut",
                "Please provide shortcut type, name and args");
        }

        // only type
        var shortcutType = Enum.TryParse<ShortcutType>(args[0], true, out var type) ? type : ShortcutType.Unspecified;

        if (shortcutType == ShortcutType.Unspecified)
        {
            return ResultExtensions.SingleResult("Invalid shortcut type",
                "Available types: " + string.Join(", ", Enum.GetNames(typeof(ShortcutType)).Select(x => x.ToLower())));
        }

        if (args.Count == 1)
        {
            return ResultExtensions.SingleResult($"Add {shortcutType.ToString().ToLower()} type shortcut",
                "Please provide shortcut name and path");
        }

        if (args.Count == 2)
        {
            return ResultExtensions.SingleResult($"Add '{args[1]}' {shortcutType.ToString().ToLower()} shortcut",
                "Please provide shortcut args");
        }

        // only type and name
        if (args.Count >= 3)
        {
            return _shortcutsService.AddShortcut(args[1], args[2], shortcutType);
        }


        return ResultExtensions.SingleResult("Invalid arguments", "Please provide shortcut type, name and path");
    }

    private List<Result> ParseRemoveShortcutCommand(models.Query query)
    {
        var args = CommandLineExtensions.SplitArguments(query.Args);

        if (args.Count < 1)
        {
            return ResultExtensions.SingleResult("Invalid arguments", "Please provide shortcut name");
        }

        return _shortcutsService.RemoveShortcut(args[0]);
    }

    private List<Result> ParseGetShortcutPathCommand(models.Query query)
    {
        var args = CommandLineExtensions.SplitArguments(query.Args);

        if (args.Count < 1)
        {
            return ResultExtensions.SingleResult("Invalid arguments", "Please provide shortcut name");
        }

        return _shortcutsService.GetShortcutDetails(args[0]);
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
                Cli.Wrap("powershell")
                   .WithArguments(path)
                   .ExecuteAsync();
            }
        );
    }


    private List<Result> ReloadCommand()
    {
        return ResultExtensions.SingleResult(Resources.SettingsManager_ReloadCommand_Reload_plugin,
            action: ReloadPluginData);
    }

    private List<Result> HelpCommand()
    {
        var helpers = _helpersRepository.GetHelpers();
        var pluginKeyword = string.Join(' ', _context.CurrentPluginMetadata.ActionKeywords);

        return _commandsWithParams.Keys
                                  .Union(_commandsWithoutParams.Keys)
                                  .Select(key =>
                                      new Result
                                      {
                                          Title = $"{pluginKeyword} " +
                                                  helpers.FirstOrDefault(z => z.Keyword.Equals(key))?.Example,
                                          SubTitle = helpers.Find(z => z.Keyword.Equals(key))?.Description,
                                          IcoPath = "images\\icon.png",
                                          Action = _ => true
                                      })
                                  .ToList();
    }
}