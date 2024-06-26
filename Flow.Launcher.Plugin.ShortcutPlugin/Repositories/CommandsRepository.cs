﻿using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class CommandsRepository : ICommandsRepository
{
    private readonly Dictionary<string, Command> _commands =
        new(StringComparer.InvariantCultureIgnoreCase);

    private readonly PluginInitContext _context;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IShortcutsService _shortcutsService;

    public CommandsRepository(
        IEnumerable<ICommand> commands,
        PluginInitContext context,
        IShortcutsRepository shortcutsRepository,
        IShortcutsService shortcutsService
    )
    {
        _context = context;
        _shortcutsRepository = shortcutsRepository;
        _shortcutsService = shortcutsService;

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
            return ShowAvailableCommands(query.ActionKeyword);
        }

        // In case this is a shortcut command, let's open shortcut
        if (_shortcutsRepository.GetShortcuts(arguments[0]) is not null)
        {
            return _shortcutsService.OpenShortcuts(arguments[0], arguments.Skip(1).ToList()); // Skips shortcut name
        }

        // If command was not found
        if (!_commands.TryGetValue(arguments[0], out var command))
        {
            // Show possible shortcuts
            var possibleShortcuts = _shortcutsRepository
                                    .GetPossibleShortcuts(arguments[0])
                                    .Select(s =>
                                        _shortcutsService.OpenShortcut(s, arguments.Skip(1).ToList())
                                                         .First()
                                    );

            // Return possible command matches
            var possibleCommands = GetPossibleCommands(arguments[0], query.ActionKeyword);

            var possibleResults = possibleShortcuts.Concat(possibleCommands).ToList();

            return possibleResults.Count != 0
                ? possibleResults
                : ResultExtensions.SingleResult(
                    "Invalid input",
                    "Please provide valid command or shortcut"
                );
        }

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
        if (executor.Arguments.Count != 0)
        {
            // Can't check because Flow Launcher trims the query
            /* if (!query.EndsWith(" "))
            {
                return ResultExtensions.SingleResult(executor.ResponseInfo.Item1, executor.ResponseInfo.Item2);
            } */

            return executor
                   .Arguments.Cast<Argument>()
                   .Select(a =>
                       ResultExtensions.Result(
                           a.ResponseInfo.Item1,
                           a.ResponseInfo.Item2,
                           () => { _context.API.ChangeQuery($"{a.Key} "); }
                       )
                   )
                   .ToList();
        }

        return Map(executor, executor.ResponseFailure, arguments);
    }

    private List<Result> ShowAvailableCommands(string actionKeyword)
    {
        return _commands
               .Values.Select(c => new Result
               {
                   Title = c.ResponseInfo.Item1 + "  ", // FIXME: Wrong order without space
                   SubTitle = c.ResponseInfo.Item2,
                   IcoPath = Icons.Logo,
                   Score = 1000 - _commands.Count,
                   Action = _ =>
                   {
                       _context.API.ChangeQuery($"{actionKeyword} {c.Key}");
                       return false;
                   }
               })
               .ToList();
    }

    private IEnumerable<Result> GetPossibleCommands(string query, string actionKeyword)
    {
        return _commands
               .Values.Where(c =>
                   c.Key.StartsWith(query, StringComparison.InvariantCultureIgnoreCase)
               )
               .Select(c =>
                   ResultExtensions.Result(
                       c.ResponseInfo.Item1,
                       c.ResponseInfo.Item2,
                       score: 1000,
                       hideAfterAction: false,
                       action: () => { _context.API.ChangeQuery($"{actionKeyword} {c.Key}"); }
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