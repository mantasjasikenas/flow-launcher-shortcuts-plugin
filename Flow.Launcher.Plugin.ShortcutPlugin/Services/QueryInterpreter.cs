using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class QueryInterpreter : IQueryInterpreter
{
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly ICommandsRepository _commandsRepository;
    private readonly IShortcutsService _shortcutsService;
    private readonly IPluginManager _pluginManager;
    private readonly IQueryParser _queryParser;

    public QueryInterpreter(IShortcutsRepository shortcutsRepository, IShortcutsService shortcutsService,
        IPluginManager pluginManager, ICommandsRepository commandsRepository, IQueryParser queryParser)
    {
        _shortcutsRepository = shortcutsRepository;
        _shortcutsService = shortcutsService;
        _pluginManager = pluginManager;
        _commandsRepository = commandsRepository;
        _queryParser = queryParser;
    }

    public List<Result> Interpret(Query query)
    {
        var parsedQuery = _queryParser.Parse(query);
        return Interpret(parsedQuery);
    }

    private List<Result> Interpret(ParsedQuery parsedQuery)
    {
        if (string.IsNullOrWhiteSpace(parsedQuery.Query.Search))
        {
            return _commandsRepository.ShowAvailableCommands();
        }

        // In case this is a shortcut command, let's open shortcut
        var firstTerm = parsedQuery.FirstTerm;
        if (_shortcutsRepository.TryGetShortcuts(firstTerm, out var shortcuts))
        {
            return _shortcutsService.OpenShortcuts(shortcuts, parsedQuery.Arguments, true);
        }

        // If command is found
        var commandKey = firstTerm.Split(" ").First();
        if (_commandsRepository.TryGetCommand(commandKey, out var command))
        {
            return ExecuteCommand(parsedQuery, command);
        }

        // If command and shortcut are not found
        var possibleResults = GetPossibleResults(firstTerm, commandKey, parsedQuery.Arguments);

        return possibleResults.Any()
            ? possibleResults
            : ResultExtensions.SingleResult(
                "Invalid input",
                "Please provide valid command or shortcut"
            );
    }

    private List<Result> GetPossibleResults(string shortcutKey, string commandKey,
        IReadOnlyDictionary<string, string> arguments)
    {
        var commands = _commandsRepository.GetPossibleCommands(commandKey);
        var shortcuts = _shortcutsRepository
                        .GetPossibleShortcuts(shortcutKey)
                        .SelectMany(s =>
                            _shortcutsService.OpenShortcut(s, arguments, false));

        return shortcuts.Concat(commands).ToList();
    }

    private List<Result> ExecuteCommand(ParsedQuery parsedQuery, Command command)
    {
        var commandArguments = parsedQuery.CommandArguments;

        var level = 0;
        var (executorOld, executorNew) = GetExecutors(null, command, commandArguments, ref level);
        var executor = executorNew ?? executorOld;

        // More arguments than needed and ...
        if (level < commandArguments.Count - 1 && executor is {AllowsMultipleValuesForSingleArgument: false})
        {
            return ResultExtensions.SingleResult(
                "Invalid command arguments",
                "Please provide valid command arguments"
            );
        }

        // If command is valid and has a handler
        if (executor?.Handler is not null)
        {
            return Map(executor, executor.ResponseSuccess, parsedQuery);
        }

        // If command has more arguments
        if (executor?.Arguments != null && executor.Arguments.Any())
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
                               : $"{parsedQuery.Query.RawQuery}"
                       )
                   )
                   .ToList();
        }

        return Map(executor, executor?.ResponseFailure, parsedQuery);
    }

    private static List<Result> Map(
        IQueryExecutor? executor,
        (string, string)? response,
        ParsedQuery parsedQuery
    )
    {
        if (executor?.Handler is not null)
        {
            return executor.Handler.Invoke(new ActionContext(), parsedQuery);
        }

        if (response is null)
        {
            return ResultExtensions.SingleResult("Something went wrong", "Please try again");
        }

        var (title, subtitle) = response.Value;
        return ResultExtensions.SingleResult(title, subtitle);
    }

    private static (IQueryExecutor?, IQueryExecutor?) GetExecutors(
        IQueryExecutor? executorOld,
        IQueryExecutor? executorNew,
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

    private static IQueryExecutor? GetExecutorFromArguments(
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