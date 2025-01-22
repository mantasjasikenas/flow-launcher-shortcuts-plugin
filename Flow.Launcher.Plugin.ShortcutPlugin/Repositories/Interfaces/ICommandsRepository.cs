using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface ICommandsRepository
{
    bool TryGetCommand(string key, out Command command);
    
    List<Result> ShowAvailableCommands();

    IEnumerable<Result> GetPossibleCommands(string query);
}
