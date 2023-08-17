using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface ICommandsRepository
{
    List<Result> ResolveCommand(List<string> arguments);
}