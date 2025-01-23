using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class CommandsRepository : ICommandsRepository
{
    private readonly Dictionary<string, Command> _commands = new(StringComparer.InvariantCultureIgnoreCase);

    private readonly IPluginManager _pluginManager;
    
    public CommandsRepository(
        IEnumerable<ICommand> commands,
        IPluginManager pluginManager
    )
    {
        _pluginManager = pluginManager;

        RegisterCommands(commands);
    }

    private void RegisterCommands(IEnumerable<ICommand> commands)
    {
        commands.Select(c => c.Create()).ToList().ForEach(c => _commands.Add(c.Key, c));
    }

    public List<Result> ShowAvailableCommands()
    {
        return _commands
               .Values.Select(c =>
                   ResultExtensions.Result(
                       title: c.ResponseInfo.Item1,
                       subtitle: c.ResponseInfo.Item2,
                       score: 1000 - _commands.Count,
                       hideAfterAction: false,
                       action: () =>
                       {
                           _pluginManager.ChangeQueryWithAppendedKeyword(c.Key);
                       },
                       autoCompleteText: _pluginManager.AppendActionKeyword(c.Key)
                   )
               )
               .ToList();
    }

    public IEnumerable<Result> GetPossibleCommands(string query)
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
                       action: () =>
                       {
                           _pluginManager.ChangeQueryWithAppendedKeyword(c.Key);
                       },
                       autoCompleteText: _pluginManager.AppendActionKeyword(c.Key)
                   )
               )
               .ToList();
    }

    public bool TryGetCommand(string key, [MaybeNullWhen(false)] out Command command)
    {
        return _commands.TryGetValue(key, out command);
    }
}