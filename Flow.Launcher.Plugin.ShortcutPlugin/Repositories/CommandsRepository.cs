using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using JetBrains.Annotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

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
            return ShowAvailableCommands(arguments);
        }

        // In case this is a shortcut command, let's open shortcut
        if (_shortcutsRepository.GetShortcut(arguments[0]) is not null)
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
                                .Select(c => Map(c, c.ResponseInfo, arguments))
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
            return new List<Result>
            {
                Map(executor, executor.ResponseSuccess, arguments)
            };
        }

        if (executor.Arguments.Count != 0)
        {
            return executor.Arguments
                           .Cast<Argument>()
                           .Select(a => Map(a, a.ResponseInfo, arguments))
                           .ToList();
        }

        return new List<Result>
        {
            Map(executor, executor.ResponseFailure, arguments)
        };
    }

    private List<Result> ShowAvailableCommands(List<string> arguments)
    {
        return _commands.Values
                        .Select(c => Map(c, c.ResponseInfo, arguments))
                        .ToList();
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

    private static Result Map(IQueryExecutor executor, (string, string)? response, List<string> arguments)
    {
        return new Result
        {
            Action = c => executor.Handler?.Invoke(c, arguments) ?? true,
            Title = response?.Item1,
            SubTitle = response?.Item2,
            IcoPath = Constants.IconPath
        };
    }

    private void RegisterCommands()
    {
        _commands.Add("add", CreateAddCommand());
        _commands.Add("reload", CreateReloadCommand());
    }

    private Command CreateAddCommand() => new()
    {
        Key = "add",
        ResponseInfo = ("add", "Add shortcuts to the list"),
        ResponseFailure = ("Enter shortcut type", "<Directory/File/Url/Plugin/Program>"),
        Arguments = GetShortcutTypes()
    };

    private Command CreateReloadCommand() => new()
    {
        Key = "reload",
        ResponseInfo = ("reload", "Reload plugin data"),
        ResponseSuccess = ("Reload", "Reload plugin data"),
        ResponseFailure = ("Failed to reload", "Something went wrong"),
        Handler = ReloadCommandHandler
    };

    private List<IQueryExecutor> GetShortcutTypes() => new()
    {
        CreateShortcutType("directory", a => new DirectoryShortcut {Key = a[2], Path = a[3]}),
        CreateShortcutType("file", a => new FileShortcut {Key = a[2], Path = a[3]}),
        CreateShortcutType("url", a => new UrlShortcut {Key = a[2], Url = a[3]}),
        CreateShellShortcut()
    };


    private IQueryExecutor CreateShortcutType(string type, Func<List<string>, Shortcut> createShortcut)
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
                            Handler = (_, a) =>
                            {
                                _shortcutsRepository.AddShortcut(createShortcut(a));
                                return true;
                            }
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
                                            Handler = (_, a) =>
                                            {
                                                _shortcutsRepository.AddShortcut(new ShellShortcut
                                                {
                                                    Key = a[2],
                                                    TargetFilePath = a[3],
                                                    Command = a[4],
                                                    Silent = a[5]
                                                        .Equals("true",
                                                            StringComparison.InvariantCultureIgnoreCase)
                                                });

                                                return true;
                                            }
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

    private bool ReloadCommandHandler(ActionContext context, List<string> arguments)
    {
        //ReloadPluginData();
        _context.API.ShowMsg("Reloaded", "Reloaded plugin data", Constants.IconPath);
        return true;
    }
}