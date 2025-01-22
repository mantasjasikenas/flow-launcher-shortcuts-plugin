using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class CommandsRepository : ICommandsRepository
{
    private readonly Dictionary<string, Command> _commands =
        new(StringComparer.InvariantCultureIgnoreCase);

    private readonly IPluginManager _pluginManager;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IShortcutsService _shortcutsService;

    public CommandsRepository(
        IEnumerable<ICommand> commands,
        IShortcutsRepository shortcutsRepository,
        IShortcutsService shortcutsService,
        IPluginManager pluginManager
    )
    {
        _shortcutsRepository = shortcutsRepository;
        _shortcutsService = shortcutsService;
        _pluginManager = pluginManager;

        RegisterCommands(commands);
    }

    private void RegisterCommands(IEnumerable<ICommand> commands)
    {
        commands.Select(c => c.Create()).ToList().ForEach(c => _commands.Add(c.Key, c));
    }

    public List<Result> ResolveCommand(List<string> arguments, Query query)
    {
        if (arguments.Count == 0)
        {
            return ShowAvailableCommands();
        }

        // FIXME only takes first argument as key
        var key = arguments[0];
        var argsWithoutKey = arguments.Skip(1).ToList();

        // In case this is a shortcut command, let's open shortcut
        if (_shortcutsRepository.TryGetShortcuts(key, out var shortcuts))
        {
            return _shortcutsService.OpenShortcuts(shortcuts, argsWithoutKey, true);
        }

        // If command was not found
        if (!_commands.TryGetValue(key, out var command))
        {
            // Show possible shortcuts
            var possibleShortcuts = _shortcutsRepository
                                    .GetPossibleShortcuts(key)
                                    .SelectMany(s => _shortcutsService.OpenShortcut(s, argsWithoutKey, false));

            // Return possible command matches
            var possibleCommands = GetPossibleCommands(key);

            var possibleResults = possibleShortcuts.Concat(possibleCommands).ToList();

            return possibleResults.Any()
                ? possibleResults
                : ResultExtensions.SingleResult(
                    "Invalid input",
                    "Please provide valid command or shortcut"
                );
        }

        // If command found
        var level = 0;
        var (executorOld, executorNew) = GetExecutors(null, command, arguments, ref level);
        var executor = executorNew ?? executorOld;

        // More arguments than needed and ...
        if (level < arguments.Count - 1 && !executor.AllowsMultipleValuesForSingleArgument)
        {
            return ResultExtensions.SingleResult(
                "Invalid command arguments",
                "Please provide valid command arguments"
            );
        }

        // If command is valid and has a handler
        if (executor.Handler is not null)
        {
            return Map(executor, executor.ResponseSuccess, arguments);
        }

        // If command has more arguments
        if (executor.Arguments.Any())
        {
            // Can't check because Flow Launcher trims the query
            /*if (!query.RawQuery.EndsWith(" "))
            {
                return ResultExtensions.SingleResult(executor.ResponseInfo.Item1, executor.ResponseInfo.Item2);
            }*/

            return executor
                   .Arguments.Cast<Argument>()
                   .Select(argument =>
                       ResultExtensions.Result(
                           title: argument.ResponseInfo.Item1,
                           subtitle: argument.ResponseInfo.Item2,
                           () =>
                           {
                               if (argument is ArgumentLiteral)
                               {
                                   _pluginManager.ChangeQueryWithAppendedKeyword(argument.ResponseInfo.Item1);
                               }
                           },
                           hideAfterAction: false,
                           autoCompleteText: argument is ArgumentLiteral
                               ? _pluginManager.AppendActionKeyword(argument.ResponseInfo.Item1)
                               : $"{query.RawQuery}"
                       )
                   )
                   .ToList();
        }

        return Map(executor, executor.ResponseFailure, arguments);
    }

    private List<Result> ShowAvailableCommands()
    {
        return _commands
               .Values.Select(c =>
                   ResultExtensions.Result(
                       title: c.ResponseInfo.Item1,
                       subtitle: c.ResponseInfo.Item2,
                       score: 1000 - _commands.Count,
                       hideAfterAction: false,
                       action: () => { _pluginManager.ChangeQueryWithAppendedKeyword(c.Key); },
                       autoCompleteText: _pluginManager.AppendActionKeyword(c.Key)
                   )
               )
               .ToList();
    }

    private IEnumerable<Result> GetPossibleCommands(string query)
    {
        return _commands
               .Values
               .Where(c =>
                   c.Key.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
               )
               .Select(c =>
                   ResultExtensions.Result(
                       title: c.ResponseInfo.Item1,
                       subtitle: c.ResponseInfo.Item2,
                       score: 1000,
                       hideAfterAction: false,
                       action: () => { _pluginManager.ChangeQueryWithAppendedKeyword(c.Key); },
                       autoCompleteText: _pluginManager.AppendActionKeyword(c.Key)
                   )
               )
               .ToList();
    }

    private static List<Result> Map(
        IQueryExecutor executor,
        (string, string)? response,
        List<string> arguments
    )
    {
        return executor.Handler is null
            ? ResultExtensions.SingleResult(response?.Item1, response?.Item2)
            : executor.Handler.Invoke(null, arguments);
    }

    private static (IQueryExecutor, IQueryExecutor) GetExecutors(
        IQueryExecutor executorOld,
        IQueryExecutor executorNew,
        IReadOnlyList<string> arguments,
        ref int level
    )
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

    private static IQueryExecutor GetExecutorFromArguments(
        IReadOnlyCollection<IQueryExecutor> executors,
        IReadOnlyList<string> arguments,
        int level
    )
    {
        if (executors.Count == 0)
        {
            return null;
        }

        var argument = arguments[level];

        var argumentsLiteral = executors.OfType<ArgumentLiteral>().ToList();

        var argExecutor =
            argumentsLiteral.FirstOrDefault(a =>
                a.Key.Equals(argument, StringComparison.InvariantCultureIgnoreCase)
            ) ?? executors.Except(argumentsLiteral).FirstOrDefault();

        return argExecutor;
    }
}