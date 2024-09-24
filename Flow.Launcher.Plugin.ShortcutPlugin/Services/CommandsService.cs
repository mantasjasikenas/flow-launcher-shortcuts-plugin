using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class CommandsService : ICommandsService
{
    private readonly ICommandsRepository _commandsRepository;

    public CommandsService(ICommandsRepository commandsRepository)
    {
        _commandsRepository = commandsRepository;
    }

    public List<Result> ResolveCommand(List<string> arguments, Query query)
    {
        return _commandsRepository.ResolveCommand(arguments, query);
    }
}