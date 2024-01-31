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
        return new Command
        {
            Key = "group",
            ResponseInfo = ("group", "Manage shortcuts group"),
            ResponseFailure = ("Failed to manage shortcuts group", "Something went wrong"),
            Arguments = new List<IQueryExecutor>
            {
                new ArgumentLiteral
                {
                    Key = "add",
                    ResponseInfo = ("group add", "Add shortcuts group"),
                    ResponseFailure = ("Failed to add shortcuts group", "Something went wrong"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter group name", "How should your group be named?"),
                            Arguments = new List<IQueryExecutor>
                            {
                                new Argument
                                {
                                    ResponseInfo = ("Enter shortcuts keys", "Which shortcuts should be in the group?"),
                                    Handler = AddGroupCommandHandler,
                                    AllowsMultipleValuesForSingleArgument = true
                                }
                            }
                        }
                    }
                },
                new ArgumentLiteral
                {
                    Key = "remove",
                    ResponseInfo = ("group remove", "Remove shortcuts group"),
                    ResponseFailure = ("Failed to remove shortcuts group", "Something went wrong"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter group name", "Which group should be removed?"),
                            ResponseSuccess = ("Remove", "Your group will be removed"),
                            Handler = RemoveGroupCommandHandler
                        }
                    }
                },
                new ArgumentLiteral
                {
                    Key = "list",
                    ResponseInfo = ("group list", "List all shortcuts groups"),
                    ResponseFailure = ("Failed to list shortcuts groups", "Something went wrong"),
                    ResponseSuccess = ("List", "List all shortcuts groups"),
                    Handler = ListGroupsCommandHandler
                }
            }
        };
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
        return new Command
        {
            Key = "keyword",
            ResponseInfo = ("keyword", "Manage plugin keyword"),
            ResponseFailure = ("Failed to manage plugin keyword", "Something went wrong"),
            Arguments = new List<IQueryExecutor>
            {
                new ArgumentLiteral
                {
                    Key = "get",
                    ResponseInfo = ("keyword get", "Shows all plugin keywords"),
                    ResponseFailure = ("Failed to get plugin keyword", "Something went wrong"),
                    ResponseSuccess = ("Get", "Get plugin keyword"),
                    Handler = GetKeywordCommandHandler
                },
                new ArgumentLiteral
                {
                    Key = "set",
                    ResponseInfo = ("keyword set", "Set plugin keyword. Other keywords will be removed"),
                    ResponseFailure = ("Failed to set plugin keyword", "Something went wrong"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter new keyword", "How should your plugin be called?"),
                            ResponseSuccess = ("Set", "Your plugin keyword will be set"),
                            Handler = SetKeywordCommandHandler
                        }
                    }
                },
                new ArgumentLiteral
                {
                    Key = "add",
                    ResponseInfo = ("keyword add", "Add additional plugin keyword"),
                    ResponseFailure = ("Failed to add plugin keyword", "Something went wrong"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter new keyword", "How should your plugin be called?"),
                            ResponseSuccess = ("Add", "Your plugin keyword will be added"),
                            Handler = AddKeywordCommandHandler
                        }
                    }
                },
                new ArgumentLiteral
                {
                    Key = "remove",
                    ResponseInfo = ("keyword remove", "Remove plugin keyword"),
                    ResponseFailure = ("Failed to remove plugin keyword", "Something went wrong"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter keyword", "Which keyword should be removed?"),
                            ResponseSuccess = ("Remove", "Your plugin keyword will be removed"),
                            Handler = RemoveKeywordCommandHandler
                        }
                    }
                }
            }
        };
    }

    private List<Result> GetKeywordCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return ResultExtensions.SingleResult("Plugin keywords",
            string.Join(", ", _context.CurrentPluginMetadata.ActionKeywords));
    }

    private List<Result> SetKeywordCommandHandler(ActionContext arg1, List<string> arg2)
    {
        var newKeyword = arg2[2];

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

    private List<Result> AddKeywordCommandHandler(ActionContext arg1, List<string> arg2)
    {
        var actionKeywords = _context.CurrentPluginMetadata.ActionKeywords;
        var newKeyword = arg2[2];

        if (actionKeywords.Contains(newKeyword))
        {
            return ResultExtensions.SingleResult("Add plugin keyword", $"Keyword `{newKeyword}` already exists");
        }

        return ResultExtensions.SingleResult("Add plugin keyword", $"New keyword added will be `{newKeyword}`",
            () => { _context.API.AddActionKeyword(_context.CurrentPluginMetadata.ID, newKeyword); });
    }

    private List<Result> RemoveKeywordCommandHandler(ActionContext arg1, List<string> arg2)
    {
        var actionKeywords = _context.CurrentPluginMetadata.ActionKeywords;

        if (actionKeywords.Count == 1)
        {
            return ResultExtensions.SingleResult("Remove plugin keyword", "You can't remove the only keyword");
        }

        if (!actionKeywords.Contains(arg2[2]))
        {
            return ResultExtensions.SingleResult("Remove plugin keyword", $"Keyword `{arg2[2]}` doesn't exist");
        }

        return ResultExtensions.SingleResult("Remove plugin keyword", $"Keyword `{arg2[2]}` will be removed",
            () => { _context.API.RemoveActionKeyword(_context.CurrentPluginMetadata.ID, arg2[2]); });
    }

    private Command CreateDuplicateCommand()
    {
        return new Command
        {
            Key = "duplicate",
            ResponseInfo = ("duplicate", "Duplicate shortcut"),
            ResponseFailure = ("Failed to duplicate shortcut", "Something went wrong"),
            Arguments = new List<IQueryExecutor>
            {
                new Argument
                {
                    ResponseInfo = ("Enter shortcut name", "Which shortcut should be duplicated?"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter new shortcut name", "How should your shortcut be named?"),
                            ResponseSuccess = ("Duplicate", "Your shortcut will be duplicated"),
                            Handler = DuplicateCommandHandler
                        }
                    }
                }
            }
        };
    }

    private List<Result> DuplicateCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return _shortcutsService.DuplicateShortcut(arg2[1], arg2[2]);
    }

    private Command CreateRemoveCommand()
    {
        return new Command
        {
            Key = "remove",
            ResponseInfo = ("remove", "Remove shortcuts from the list"),
            ResponseFailure = ("Enter shortcut name", "Which shortcut should be removed?"),
            Arguments = new List<IQueryExecutor>
            {
                new Argument
                {
                    ResponseInfo = ("Enter shortcut name", "Which shortcut should be removed?"),
                    ResponseSuccess = ("Remove", "Your shortcut will be removed from the list"),
                    Handler = RemoveCommandHandler
                }
            }
        };
    }

    private List<Result> RemoveCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return _shortcutsService.RemoveShortcut(arg2[1]);
    }

    private Command CreateVariablesCommand()
    {
        return new Command
        {
            Key = "var",
            ResponseInfo = ("var", "Manage variables"),
            ResponseFailure = ("Failed to manage variables", "Something went wrong"),
            Arguments = new List<IQueryExecutor>
            {
                new ArgumentLiteral
                {
                    Key = "add",
                    ResponseInfo = ("var add", "Add variable"),
                    ResponseFailure = ("Failed to add variable", "Something went wrong"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter variable name", "How should your variable be named?"),
                            Arguments = new List<IQueryExecutor>
                            {
                                new Argument
                                {
                                    ResponseInfo = ("Enter variable value", "What should your variable value be?"),
                                    Handler = AddVariableCommandHandler
                                }
                            }
                        }
                    }
                },
                new ArgumentLiteral
                {
                    Key = "remove",
                    ResponseInfo = ("var remove", "Remove variable"),
                    ResponseFailure = ("Failed to remove variable", "Something went wrong"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter variable name", "Which variable should be removed?"),
                            ResponseSuccess = ("Remove", "Your variable will be removed from the list"),
                            Handler = RemoveVariableCommandHandler
                        }
                    }
                },
                new ArgumentLiteral
                {
                    Key = "list",
                    ResponseInfo = ("var list", "List all variables"),
                    ResponseFailure = ("Failed to list variables", "Something went wrong"),
                    ResponseSuccess = ("List", "List all variables"),
                    Handler = ListVariablesCommandHandler
                }
            }
        };
    }

    private List<Result> ListVariablesCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return _variablesService.GetVariables();
    }

    private List<Result> RemoveVariableCommandHandler(ActionContext arg1, List<string> arg2)
    {
        // q var remove <name>
        return _variablesService.RemoveVariable(arg2[2]);
    }

    private List<Result> AddVariableCommandHandler(ActionContext arg1, List<string> arg2)
    {
        // q var add <name> <value>
        return _variablesService.AddVariable(arg2[2], arg2[3]);
    }


    private Command CreateExportCommand()
    {
        return new Command
        {
            Key = "export",
            ResponseInfo = ("export", "Export shortcuts"),
            ResponseFailure = ("Failed to export shortcuts", "Something went wrong"),
            Handler = ExportCommandHandler
        };
    }

    private List<Result> ExportCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return _shortcutsService.ExportShortcuts();
    }

    private Command CreateImportCommand()
    {
        return new Command
        {
            Key = "import",
            ResponseInfo = ("import", "Import shortcuts"),
            ResponseFailure = ("Failed to import shortcuts", "Something went wrong"),
            Handler = ImportCommandHandler
        };
    }

    private List<Result> ImportCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return _shortcutsService.ImportShortcuts();
    }

    private Command CreateConfigCommand()
    {
        return new Command
        {
            Key = "config",
            ResponseInfo = ("config", "Configure plugin"),
            ResponseFailure = ("Failed to open config", "Something went wrong"),
            Handler = ConfigCommandHandler
        };
    }

    private List<Result> ConfigCommandHandler(ActionContext arg1, List<string> arg2)
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
        return new Command
        {
            Key = "list",
            ResponseInfo = ("list", "List all shortcuts"),
            ResponseFailure = ("Failed to show all shortcuts", "Something went wrong"),
            ResponseSuccess = ("List", "List all shortcuts"),
            AllowsMultipleValuesForSingleArgument = true,
            Handler = ListCommandHandler
        };
    }

    private List<Result> ListCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return _shortcutsService.GetShortcuts(arg2);
    }

    private Command CreateAddCommand()
    {
        return new Command
        {
            Key = "add",
            ResponseInfo = ("add", "Add shortcuts to the list"),
            ResponseFailure = ("Enter shortcut type", "Which type of shortcut do you want to add?"),
            Arguments = GetShortcutTypes()
        };
    }

    private Command CreateReloadCommand()
    {
        return new Command
        {
            Key = "reload",
            ResponseInfo = ("reload", "Reload plugin data"),
            ResponseSuccess = ("Reload", "Reload plugin data"),
            ResponseFailure = ("Failed to reload", "Something went wrong"),
            Handler = ReloadCommandHandler
        };
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
        return new Command
        {
            Key = "settings",
            ResponseInfo = ("settings", "Open settings"),
            ResponseFailure = ("Failed to open settings", "Something went wrong"),
            Handler = SettingsCommandHandler
        };
    }

    private List<Result> SettingsCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return ResultExtensions.SingleResult("Open Flow Launcher settings", "",
            () => { _context.API.OpenSettingDialog(); });
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
        return ResultExtensions.SingleResult("Creating url shortcut", $"Url: {arguments[3]}",
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

    private List<Result> CreateFileShortcutHandler(ActionContext arg1, List<string> arg2)
    {
        return ResultExtensions.SingleResult("Creating file shortcut", $"File path: {arg2[3]}", () =>
        {
            var key = arg2[2];
            var filePath = arg2[3];

            _shortcutsRepository.AddShortcut(new FileShortcut
            {
                Key = key,
                Path = filePath
            });
        });
    }

    private List<Result> CreateDirectoryShortcutHandler(ActionContext arg1, List<string> arg2)
    {
        return ResultExtensions.SingleResult("Creating directory shortcut", $"Directory path: {arg2[3]}",
            () =>
            {
                var key = arg2[2];
                var directoryPath = arg2[3];

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

        return ResultExtensions.SingleResult("Creating shell shortcut", subtitle, () =>
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


    private IQueryExecutor CreateShortcutType(string type,
        Func<ActionContext, List<string>, List<Result>> createShortcutHandler)
    {
        return new ArgumentLiteral
        {
            Key = type,
            ResponseFailure = ("Enter shortcut name", "How should your shortcut be named?"),
            ResponseInfo = ($"add {type}", ""),
            Arguments = new List<IQueryExecutor>
            {
                new Argument
                {
                    ResponseFailure = ("Enter shortcut path", "This is where your shortcut will point to"),
                    ResponseInfo = ("Enter shortcut name", "How should your shortcut be named?"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseSuccess = ("Add", "Your new shortcut will be addded to the list"),
                            ResponseInfo = ("Enter shortcut path", "This is where your shortcut will point to"),
                            Handler = createShortcutHandler
                        }
                    }
                }
            }
        };
    }

    private IQueryExecutor CreateShellShortcut()
    {
        return new ArgumentLiteral
        {
            Key = "shell",
            ResponseInfo = ("add shell", ""),
            Arguments = new List<IQueryExecutor>
            {
                new Argument
                {
                    ResponseInfo = ("Enter shell type", "Which shell should be used? (cmd/powershell)"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter shortcut name", "How should your shortcut be named?"),
                            Arguments = new List<IQueryExecutor>
                            {
                                new Argument
                                {
                                    ResponseInfo = ("Should execution be silent?",
                                        "Should execution be silent? (true/false)"),
                                    Arguments = new List<IQueryExecutor>
                                    {
                                        new Argument
                                        {
                                            ResponseInfo = ("Enter shell arguments",
                                                "What should your shell arguments be?"),
                                            Handler = CreateShellShortcutHandler,
                                            AllowsMultipleValuesForSingleArgument = true
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}