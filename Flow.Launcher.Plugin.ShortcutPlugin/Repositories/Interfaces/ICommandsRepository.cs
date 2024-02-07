using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface ICommandsRepository
{
    List<Result> ResolveCommand(List<string> arguments, Query query);
}
