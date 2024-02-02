using System;
using System.Collections.Generic;
using System.Linq;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
using FuzzySharp;
using JetBrains.Annotations;
using Command = Flow.Launcher.Plugin.ShortcutPlugin.models.Command;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class CommandsRepository : ICommandsRepository
{
    private readonly Dictionary<string, Command> _commands;
    private readonly PluginInitContext _context;
    private readonly ISettingsService _settingsService;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IShortcutsService _shortcutsService;
    private readonly IVariablesService _variablesService;


    public CommandsRepository(PluginInitContext context, IShortcutsRepository shortcutsRepository,
        IShortcutsService shortcutsService, ISettingsService settingsService, IVariablesService variablesService)
    {
        _context = context;
        _shortcutsRepository = shortcutsRepository;
        _shortcutsService = shortcutsService;
        _settingsService = settingsService;
        _variablesService = variablesService;

        _commands = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);

        RegisterCommands();
    }

    public List<Result> ResolveCommand(List<string> arguments, string query)
    {
        if (arguments.Count == 0)
        {
            return ShowAvailableCommands();
        }

        // In case this is a shortcut command, let's open shortcut
        if (_shortcutsRepository.GetShortcut(arguments[0]) is not null)
        {
            return _shortcutsService.OpenShortcut(arguments[0], arguments.Skip(1).ToList()); // Skips shortcut name
        }

        // If command was not found
        if (!_commands.TryGetValue(arguments[0], out var command))
        {
            // Show possible shortcuts
            var possibleShortcuts = _shortcutsRepository.GetShortcuts()
                                                        .Where(s => Fuzz.PartialRatio(s.Key, arguments[0]) > 90)
                                                        .Select(s => s is GroupShortcut
                                                            ? ResultExtensions.Result(s.Key, s.Key)
                                                            : _shortcutsService.OpenShortcut(s.Key,
                                                                                    arguments.Skip(1).ToList())
                                                                                .First())
                                                        .ToList();

            // Return possible command matches
            var possibleCommands = _commands.Values
                                            .Where(c => c.Key.StartsWith(arguments[0],
                                                StringComparison.InvariantCultureIgnoreCase))
                                            .Select(c =>
                                                ResultExtensions.Result(c.ResponseInfo.Item1, c.ResponseInfo.Item2))
                                            // .DefaultIfEmpty(ResultExtensions.Result("Invalid command",
                                            //     "Please provide valid command"))
                                            .ToList();

            var possibleResults = possibleShortcuts
                                    .Concat(possibleCommands)
                                    .ToList();

            return possibleResults.Count != 0
                ? possibleResults
                : ResultExtensions.SingleResult("Invalid input", "Please provide valid command or shortcut");

            // If user has already written some arguments and the first argument is invalid
        }

        var level = 0;
        var (executorOld, executorNew) = GetExecutors(null, command, arguments, ref level);
        var executor = executorNew ?? executorOld;

        // More arguments than needed and ...
        if (level < arguments.Count - 1 && !executor.AllowsMultipleValuesForSingleArgument)
        {
            return ResultExtensions.SingleResult("Invalid command arguments", "Please provide valid command arguments");
        }

        // If command is valid and has a handler
        if (executor.Handler is not null)
        {
            return Map(executor, executor.ResponseSuccess, arguments);
        }


        // If command has more arguments
        if (executor.Arguments.Count != 0)
        {
            // Can't check because Flow Launcher trims the query
            /* if (!query.EndsWith(" "))
            {
                return ResultExtensions.SingleResult(executor.ResponseInfo.Item1, executor.ResponseInfo.Item2);
            } */

            return executor.Arguments
                            .Cast<Argument>()
                            .Select(a =>
                                ResultExtensions.Result(a.ResponseInfo.Item1, a.ResponseInfo.Item2,
                                    () => { _context.API.ChangeQuery($"{a.Key} "); }))
                            .ToList();
        }


        return Map(executor, executor.ResponseFailure, arguments);
    }

    private List<Result> ShowAvailableCommands()
    {
        return _commands.Values
                        .Select(c => new Result
                        {
                            Title = c.ResponseInfo.Item1,
                            SubTitle = c.ResponseInfo.Item2,
                            IcoPath = Icons.Logo
                        })
                        .ToList();
    }

    private static List<Result> Map(IQueryExecutor executor, (string, string)? response, List<string> arguments)
    {
        return executor.Handler is null
            ? ResultExtensions.SingleResult(response?.Item1, response?.Item2)
            : executor.Handler.Invoke(null, arguments);
    }

    private static (IQueryExecutor, IQueryExecutor) GetExecutors(IQueryExecutor executorOld, IQueryExecutor executorNew,
        IReadOnlyList<string> arguments, ref int level)
    {
        while (true)
        {
            if (executorNew is null)
            {
                return (executorOld, executorOld);
            }

            if (arguments.Count == level + 1 || executorNew.Arguments is null)
            {
                return (executorOld, executorNew);
            }

            executorOld = executorNew;
            executorNew = GetExecutorFromArguments(executorNew.Arguments, arguments, level + 1);

            level++;
        }
    }

    [CanBeNull]
    private static IQueryExecutor GetExecutorFromArguments(IReadOnlyCollection<IQueryExecutor> executors,
        IReadOnlyList<string> arguments,
        int level)
    {
        if (executors.Count == 0)
        {
            return null;
        }

        var argument = arguments[level];

        var argumentsLiteral = executors
                                .OfType<ArgumentLiteral>()
                                .ToList();

        var argExecutor =
            argumentsLiteral.FirstOrDefault(a => a.Key.Equals(argument, StringComparison.InvariantCultureIgnoreCase))
            ?? executors.Except(argumentsLiteral).FirstOrDefault();

        return argExecutor;
    }

    private void RegisterCommands()
    {
        _commands.Add("add", CreateAddCommand());
        _commands.Add("list", CreateListCommand());
        _commands.Add("reload", CreateReloadCommand());
        _commands.Add("settings", CreateSettingsCommand());
        _commands.Add("config", CreateConfigCommand());
        _commands.Add("import", CreateImportCommand());
        _commands.Add("export", CreateExportCommand());
        _commands.Add("var", CreateVariablesCommand());
        _commands.Add("remove", CreateRemoveCommand());
        _commands.Add("duplicate", CreateDuplicateCommand());
        _commands.Add("keyword", CreateKeywordCommand());
        _commands.Add("group", CreateGroupCommand());
    }

    private Command CreateGroupCommand()
    {
        var addGroupKeysArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter shortcuts keys", "Which shortcuts should be in the group?"))
            .WithHandler(AddGroupCommandHandler)
            .WithMultipleValuesForSingleArgument()
            .Build();

        var addGroupNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter group name", "How should your group be named?"))
            .WithResponseSuccess(("Add", "Your group will be added"))
            .WithArguments(addGroupKeysArgument)
            .Build();

        var addSubCommand = new ArgumentLiteralBuilder()
            .WithKey("add")
            .WithResponseInfo(("group add", "Add shortcuts group"))
            .WithResponseFailure(("Failed to add shortcuts group", "Something went wrong"))
            .WithArgument(addGroupNameArgument)
            .Build();

        var removeGroupNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter group name", "Which group should be removed?"))
            .WithResponseSuccess(("Remove", "Your group will be removed"))
            .WithHandler(RemoveGroupCommandHandler)
            .Build();

        var removeSubCommand = new ArgumentLiteralBuilder()
            .WithKey("remove")
            .WithResponseInfo(("group remove", "Remove shortcuts group"))
            .WithResponseFailure(("Failed to remove shortcuts group", "Something went wrong"))
            .WithArgument(removeGroupNameArgument)
            .Build();

        var listSubCommand = new ArgumentLiteralBuilder()
            .WithKey("list")
            .WithResponseInfo(("group list", "List all shortcuts groups"))
            .WithResponseFailure(("Failed to list shortcuts groups", "Something went wrong"))
            .WithResponseSuccess(("List", "List all shortcuts groups"))
            .WithHandler(ListGroupsCommandHandler)
            .Build();

        return new CommandBuilder()
            .WithKey("group")
            .WithResponseInfo(("group", "Manage shortcuts group"))
            .WithResponseFailure(("Failed to manage shortcuts group", "Something went wrong"))
            .WithArguments(addSubCommand, removeSubCommand, listSubCommand)
            .Build();
    }

    private List<Result> ListGroupsCommandHandler(ActionContext context, List<string> arguments)
    {
        return _shortcutsService.GetGroups();
    }

    private List<Result> RemoveGroupCommandHandler(ActionContext context, List<string> arguments)
    {
        return _shortcutsService.RemoveShortcut(arguments[2]);
    }

    private List<Result> AddGroupCommandHandler(ActionContext context, List<string> arguments)
    {
        var keys = arguments.Skip(3).ToList();

        return ResultExtensions.SingleResult("Creating group shortcut", "Keys : " + string.Join(", ", keys), () =>
        {
            var key = arguments[2];
            _shortcutsRepository.GroupShortcuts(key, keys);
        });
    }

    private Command CreateKeywordCommand()
    {
        var getSubCommand = new ArgumentLiteralBuilder()
            .WithKey("get")
            .WithResponseInfo(("keyword get", "Shows all plugin keywords"))
            .WithResponseFailure(("Failed to get plugin keyword", "Something went wrong"))
            .WithResponseSuccess(("Get", "Get plugin keyword"))
            .WithHandler(GetKeywordCommandHandler)
            .Build();

        var setArgumentKeyword = new ArgumentBuilder()
            .WithResponseInfo(("Enter new keyword", "How should your plugin be called?"))
            .WithResponseSuccess(("Set", "Your plugin keyword will be set"))
            .WithHandler(SetKeywordCommandHandler)
            .Build();

        var setSubCommand = new ArgumentLiteralBuilder()
            .WithKey("set")
            .WithResponseInfo(("keyword set", "Set plugin keyword. Other keywords will be removed"))
            .WithResponseFailure(("Failed to set plugin keyword", "Something went wrong"))
            .WithArgument(setArgumentKeyword)
            .Build();

        var addArgumentKeyword = new ArgumentBuilder()
            .WithResponseInfo(("Enter new keyword", "How should your plugin be called?"))
            .WithResponseSuccess(("Add", "Your plugin keyword will be added"))
            .WithHandler(AddKeywordCommandHandler)
            .Build();

        var addSubCommand = new ArgumentLiteralBuilder()
            .WithKey("add")
            .WithResponseInfo(("keyword add", "Add additional plugin keyword"))
            .WithResponseFailure(("Failed to add plugin keyword", "Something went wrong"))
            .WithArgument(addArgumentKeyword)
            .Build();

        var removeArgumentKeyword = new ArgumentBuilder()
            .WithResponseInfo(("Enter keyword", "Which keyword should be removed?"))
            .WithResponseSuccess(("Remove", "Your plugin keyword will be removed"))
            .WithHandler(RemoveKeywordCommandHandler)
            .Build();

        var removeSubCommand = new ArgumentLiteralBuilder()
            .WithKey("remove")
            .WithResponseInfo(("keyword remove", "Remove plugin keyword"))
            .WithResponseFailure(("Failed to remove plugin keyword", "Something went wrong"))
            .WithArgument(removeArgumentKeyword)
            .Build();

        return new CommandBuilder()
            .WithKey("keyword")
            .WithResponseInfo(("keyword", "Manage plugin keyword"))
            .WithResponseFailure(("Failed to manage plugin keyword", "Something went wrong"))
            .WithArguments(getSubCommand, setSubCommand, addSubCommand, removeSubCommand)
            .Build();
    }

    private List<Result> GetKeywordCommandHandler(ActionContext context, List<string> arguments)
    {
        return ResultExtensions.SingleResult("Plugin keywords",
            string.Join(", ", _context.CurrentPluginMetadata.ActionKeywords));
    }

    private List<Result> SetKeywordCommandHandler(ActionContext context, List<string> arguments)
    {
        var newKeyword = arguments[2];

        return ResultExtensions.SingleResult("Setting plugin keyword (other will be removed)",
            $"New keyword will be `{newKeyword}`", () =>
            {
                _context.CurrentPluginMetadata.ActionKeywords
                        .ToList()
                        .ForEach(oldKeyword =>
                        {
                            _context.API.RemoveActionKeyword(_context.CurrentPluginMetadata.ID, oldKeyword);
                        });

                _context.API.AddActionKeyword(_context.CurrentPluginMetadata.ID, newKeyword);
            });
    }

    private List<Result> AddKeywordCommandHandler(ActionContext context, List<string> arguments)
    {
        var actionKeywords = _context.CurrentPluginMetadata.ActionKeywords;
        var newKeyword = arguments[2];

        if (actionKeywords.Contains(newKeyword))
        {
            return ResultExtensions.SingleResult("Add plugin keyword", $"Keyword `{newKeyword}` already exists");
        }

        return ResultExtensions.SingleResult("Add plugin keyword", $"New keyword added will be `{newKeyword}`",
            () => { _context.API.AddActionKeyword(_context.CurrentPluginMetadata.ID, newKeyword); });
    }

    private List<Result> RemoveKeywordCommandHandler(ActionContext context, List<string> arguments)
    {
        var actionKeywords = _context.CurrentPluginMetadata.ActionKeywords;
        var newKeyword = arguments[2];

        if (!actionKeywords.Contains(newKeyword))
        {
            return ResultExtensions.SingleResult("Remove plugin keyword", $"Keyword `{newKeyword}` doesn't exist");
        }

        if (actionKeywords.Count == 1)
        {
            return ResultExtensions.SingleResult("Remove plugin keyword", "You can't remove the only keyword");
        }

        return ResultExtensions.SingleResult("Remove plugin keyword", $"Keyword `{newKeyword}` will be removed",
            () => { _context.API.RemoveActionKeyword(_context.CurrentPluginMetadata.ID, newKeyword); });
    }

    private Command CreateDuplicateCommand()
    {
        var duplicateNewNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter new shortcut name", "How should your shortcut be named?"))
            .WithResponseSuccess(("Duplicate", "Your shortcut will be duplicated"))
            .WithHandler(DuplicateCommandHandler)
            .Build();

        var duplicateCurrentNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter shortcut name", "Which shortcut should be duplicated?"))
            .WithArguments(duplicateNewNameArgument)
            .Build();

        return new CommandBuilder()
            .WithKey("duplicate")
            .WithResponseInfo(("duplicate", "Duplicate shortcut"))
            .WithResponseFailure(("Failed to duplicate shortcut", "Something went wrong"))
            .WithArgument(duplicateCurrentNameArgument)
            .Build();
    }

    private List<Result> DuplicateCommandHandler(ActionContext context, List<string> arguments)
    {
        return _shortcutsService.DuplicateShortcut(arguments[1], arguments[2]);
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

    private Command CreateVariablesCommand()
    {
        var addVariableValueArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter variable value", "What should your variable value be?"))
            .WithHandler(AddVariableCommandHandler)
            .Build();

        var addVariableNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter variable name", "How should your variable be named?"))
            .WithArgument(addVariableValueArgument)
            .Build();

        var addVariable = new ArgumentLiteralBuilder()
            .WithKey("add")
            .WithResponseInfo(("var add", "Add variable"))
            .WithResponseFailure(("Failed to add variable", "Something went wrong"))
            .WithArgument(addVariableNameArgument)
            .Build();

        var removeVariableArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter variable name", "Which variable should be removed?"))
            .WithResponseSuccess(("Remove", "Your variable will be removed from the list"))
            .WithHandler(RemoveVariableCommandHandler)
            .Build();

        var removeVariable = new ArgumentLiteralBuilder()
            .WithKey("remove")
            .WithResponseInfo(("var remove", "Remove variable"))
            .WithResponseFailure(("Failed to remove variable", "Something went wrong"))
            .WithArgument(removeVariableArgument)
            .Build();

        var listVariables = new ArgumentLiteralBuilder()
            .WithKey("list")
            .WithResponseInfo(("var list", "List all variables"))
            .WithResponseFailure(("Failed to list variables", "Something went wrong"))
            .WithResponseSuccess(("List", "List all variables"))
            .WithHandler(ListVariablesCommandHandler)
            .Build();

        return new CommandBuilder()
            .WithKey("var")
            .WithResponseInfo(("var", "Manage variables"))
            .WithResponseFailure(("Failed to manage variables", "Something went wrong"))
            .WithArguments(addVariable, removeVariable, listVariables)
            .Build();
    }

    private List<Result> ListVariablesCommandHandler(ActionContext context, List<string> arguments)
    {
        return _variablesService.GetVariables();
    }

    private List<Result> RemoveVariableCommandHandler(ActionContext context, List<string> arguments)
    {
        return _variablesService.RemoveVariable(arguments[2]);
    }

    private List<Result> AddVariableCommandHandler(ActionContext context, List<string> arguments)
    {
        return _variablesService.AddVariable(arguments[2], arguments[3]);
    }

    private Command CreateExportCommand()
    {
        return new CommandBuilder()
            .WithKey("export")
            .WithResponseInfo(("export", "Export shortcuts"))
            .WithResponseFailure(("Failed to export shortcuts", "Something went wrong"))
            .WithHandler((_, _) => _shortcutsService.ExportShortcuts())
            .Build();
    }

    private Command CreateImportCommand()
    {
        return new CommandBuilder()
            .WithKey("import")
            .WithResponseInfo(("import", "Import shortcuts"))
            .WithResponseFailure(("Failed to import shortcuts", "Something went wrong"))
            .WithHandler((_, _) => _shortcutsService.ImportShortcuts())
            .Build();
    }

    private Command CreateConfigCommand()
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

    private Command CreateListCommand()
    {
        return new CommandBuilder()
            .WithKey("list")
            .WithResponseInfo(("list", "List all shortcuts"))
            .WithResponseFailure(("Failed to show all shortcuts", "Something went wrong"))
            .WithResponseSuccess(("List", "List all shortcuts"))
            .WithMultipleValuesForSingleArgument()
            .WithHandler((_, arguments) => _shortcutsService.GetShortcuts(arguments))
            .Build();
    }

    private Command CreateAddCommand()
    {
        return new CommandBuilder()
            .WithKey("add")
            .WithResponseInfo(("add", "Add shortcuts to the list"))
            .WithResponseFailure(("Enter shortcut type", "Which type of shortcut do you want to add?"))
            .WithArguments(GetShortcutTypes())
            .Build();
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

    private List<IQueryExecutor> GetShortcutTypes()
    {
        return new List<IQueryExecutor>
        {
            CreateShortcutType("directory", CreateDirectoryShortcutHandler),
            CreateShortcutType("file", CreateFileShortcutHandler),
            CreateShortcutType("url", CreateUrlShortcutHandler),
            CreateShellShortcut()
        };
    }

    private List<Result> CreateUrlShortcutHandler(ActionContext context, List<string> arguments)
    {
        return ResultExtensions.SingleResult("Create url shortcut", $"Url: {arguments[3]}",
            () =>
            {
                var key = arguments[2];
                var url = arguments[3];

                _shortcutsRepository.AddShortcut(new UrlShortcut
                {
                    Key = key,
                    Url = url
                });
            });
    }

    private List<Result> CreateFileShortcutHandler(ActionContext context, List<string> arguments)
    {
        return ResultExtensions.SingleResult("Create file shortcut", $"File path: {arguments[3]}", () =>
        {
            var key = arguments[2];
            var filePath = arguments[3];

            _shortcutsRepository.AddShortcut(new FileShortcut
            {
                Key = key,
                Path = filePath
            });
        });
    }

    private List<Result> CreateDirectoryShortcutHandler(ActionContext context, List<string> arguments)
    {
        return ResultExtensions.SingleResult("Create directory shortcut", $"Directory path: {arguments[3]}",
            () =>
            {
                var key = arguments[2];
                var directoryPath = arguments[3];

                _shortcutsRepository.AddShortcut(new DirectoryShortcut
                {
                    Key = key,
                    Path = directoryPath
                });
            });
    }

    private List<Result> CreateShellShortcutHandler(ActionContext context, List<string> arguments)
    {
        if (arguments.Count < 6)
        {
            return ResultExtensions.SingleResult("Invalid shell shortcut arguments",
                "Please provide valid shell shortcut arguments");
        }

        if (!Enum.TryParse<ShellType>(arguments[2], true, out var shellType))
        {
            return ResultExtensions.SingleResult("Invalid shell type",
                "Please provide valid shell type (cmd/powershell)");
        }

        if (!bool.TryParse(arguments[4], out var silent))
        {
            return ResultExtensions.SingleResult("Invalid silent argument",
                "Please provide valid silent argument (true/false)");
        }

        var key = arguments[3];
        var shellArguments = string.Join(" ", arguments.Skip(5));

        var subtitles = new List<string>
    {
        $"Type: {shellType.ToString().ToLower()}",
        $"key: {key}",
        $"silent: {silent.ToString().ToLower()}",
        $"command: {shellArguments}"
    };
        var subtitle = string.Join(", ", subtitles);

        return ResultExtensions.SingleResult("Create shell shortcut", subtitle, () =>
        {
            _shortcutsRepository.AddShortcut(new ShellShortcut
            {
                Key = key,
                ShellType = shellType,
                Silent = silent,
                Arguments = shellArguments
            });
        });
    }

    private static IQueryExecutor CreateShortcutType(string type,
        Func<ActionContext, List<string>, List<Result>> createShortcutHandler)
    {
        var createShortcutHandlerArgument = new ArgumentBuilder()
            .WithResponseSuccess(("Add", "Your new shortcut will be added to the list"))
            .WithResponseInfo(("Enter shortcut path", "This is where your shortcut will point to"))
            .WithHandler(createShortcutHandler)
            .Build();

        var shortcutNameArgument = new ArgumentBuilder()
            .WithResponseFailure(("Enter shortcut path", "This is where your shortcut will point to"))
            .WithResponseInfo(("Enter shortcut name", "How should your shortcut be named?"))
            .WithArgument(createShortcutHandlerArgument)
            .Build();

        return new ArgumentLiteralBuilder()
            .WithKey(type)
            .WithResponseFailure(("Enter shortcut name", "How should your shortcut be named?"))
            .WithResponseInfo(($"add {type}", ""))
            .WithArgument(shortcutNameArgument)
            .Build();
    }

    private IQueryExecutor CreateShellShortcut()
    {
        var shellArgumentsArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter shell arguments", "What should your shell arguments be?"))
            .WithHandler(CreateShellShortcutHandler)
            .WithMultipleValuesForSingleArgument()
            .Build();

        var silentExecutionArgument = new ArgumentBuilder()
            .WithResponseInfo(("Should execution be silent?", "Should execution be silent? (true/false)"))
            .WithArgument(shellArgumentsArgument)
            .Build();

        var shortcutNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter shortcut name", "How should your shortcut be named?"))
            .WithArgument(silentExecutionArgument)
            .Build();

        var shellTypeArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter shell type", "Which shell should be used? (cmd/powershell)"))
            .WithArgument(shortcutNameArgument)
            .Build();

        return new ArgumentLiteralBuilder()
            .WithKey("shell")
            .WithResponseInfo(("add shell", ""))
            .WithArgument(shellTypeArgument)
            .Build();
    }
}