using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class CommandsRepository : ICommandsRepository
{
    private readonly Dictionary<string, Command> _commands;

    private readonly PluginInitContext _context;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IShortcutsService _shortcutsService;

    public CommandsRepository(PluginInitContext context, IShortcutsRepository shortcutsRepository,
        IShortcutsService shortcutsService)
    {
        _context = context;
        _shortcutsRepository = shortcutsRepository;
        _shortcutsService = shortcutsService;

        _commands = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);

        RegisterCommands();
    }


    public List<Result> ResolveCommand(List<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return ShowAvailableCommands();
        }

        // In case this is a shortcut command, let's open shortcut
        if (arguments.Count == 1 && _shortcutsRepository.GetShortcut(arguments[0]) is not null)
        {
            return _shortcutsService.OpenShortcut(arguments[0]);
        }

        // If command was not found
        if (!_commands.TryGetValue(arguments[0], out var command))
        {
            // If user is still writing the command
            if (arguments.Count == 1)
            {
                // Return possible command matches
                return _commands.Values
                                .Where(c => c.Key.StartsWith(arguments[0], StringComparison.InvariantCultureIgnoreCase))
                                .Select(c => ResultExtensions.Result(c.ResponseInfo.Item1, c.ResponseInfo.Item2))
                                .DefaultIfEmpty(ResultExtensions.Result("Invalid command",
                                    "Please provide valid command"))
                                .ToList();
            }

            // If user has already written some arguments and the first argument is invalid
            return ResultExtensions.SingleResult("Invalid command", "Please provide valid command");
        }

        var level = 0;
        var (executorOld, executorNew) = GetExecutors(null, command, arguments, ref level);
        var executor = executorNew ?? executorOld;

        // more arguments than needed
        if (level < arguments.Count - 1)
        {
            return ResultExtensions.SingleResult("Invalid command arguments", "Please provide valid command arguments");
        }

        // If command is valid and has a handler
        if (executor.Handler is not null)
        {
            return MapResults(executor, executor.ResponseSuccess, arguments);
        }

        // If command is ...
        if (executor.Arguments.Count != 0)
        {
            return executor.Arguments
                           .Cast<Argument>()
                           .Select(a =>
                               ResultExtensions.Result(a.ResponseInfo.Item1, a.ResponseInfo.Item2,
                                   () => { _context.API.ChangeQuery($"{a.Key} "); }))
                           .ToList();
        }


        return MapResults(executor, executor.ResponseFailure, arguments);
    }

    private List<Result> ShowAvailableCommands()
    {
        return _commands.Values
                        .Select(c => ResultExtensions.Result(c.ResponseInfo.Item1, c.ResponseInfo.Item2, () =>
                        {
                            //_context.API.ChangeQuery($"{c.Key}");
                        }))
                        .ToList();
    }

    private static List<Result> MapResults(IQueryExecutor executor, (string, string)? response, List<string> arguments)
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
        _commands.Add("reload", CreateReloadCommand());
    }

    private Command CreateAddCommand()
    {
        return new Command
        {
            Key = "add",
            ResponseInfo = ("add", "Add shortcuts to the list"),
            ResponseFailure = ("Enter shortcut type", "<Directory/File/Url/Plugin/Program>"),
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
        return new List<Result>
        {
            new()
            {
                Title = "Reload1",
                SubTitle = "Reloads plugin data"
            },
            new()
            {
                Title = "Reload2",
                SubTitle = "Reloads plugin data"
            }
        };
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

    private List<Result> CreateUrlShortcutHandler(ActionContext arg1, List<string> arguments)
    {
        return ResultExtensions.SingleResult("Creating url shortcut", "", () =>
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
        return ResultExtensions.SingleResult("Creating file shortcut", "", () =>
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
        return ResultExtensions.SingleResult("Creating directory shortcut", "", () =>
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
        _shortcutsRepository.AddShortcut(new ShellShortcut
        {
            Key = arguments[2],
            TargetFilePath = arguments[3],
            Command = arguments[4],
            Silent = arguments[5]
                .Equals("true",
                    StringComparison.InvariantCultureIgnoreCase)
        });

        throw new NotImplementedException();
    }


    private IQueryExecutor CreateShortcutType(string type,
        Func<ActionContext, List<string>, List<Result>> createShortcutHandler)
    {
        return new ArgumentLiteral
        {
            Key = type,
            ResponseFailure = ("Enter shortcut name", "How should your shortcut be named?"),
            ResponseInfo = ($"add {type}", "<Directory/File/Url/Plugin/Program>"),
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
            ResponseInfo = ("add shell", "<Directory/File/Url/Plugin/Program>"),
            Arguments = new List<IQueryExecutor>
            {
                new Argument
                {
                    ResponseInfo = ("Enter shortcut name", "How should your shortcut be named?"),
                    Arguments = new List<IQueryExecutor>
                    {
                        new Argument
                        {
                            ResponseInfo = ("Enter target file path", "This is where your shell will point to"),
                            Arguments = new List<IQueryExecutor>
                            {
                                new Argument
                                {
                                    ResponseInfo = ("Enter shell command",
                                        "This is what your shell will execute"),
                                    Arguments = new List<IQueryExecutor>
                                    {
                                        new Argument
                                        {
                                            ResponseSuccess = ("Add",
                                                "Your new shortcut will be added to the list"),
                                            ResponseInfo = ("Enter shell silent",
                                                "Should your shell be silent?"),
                                            Handler = CreateShellShortcutHandler
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