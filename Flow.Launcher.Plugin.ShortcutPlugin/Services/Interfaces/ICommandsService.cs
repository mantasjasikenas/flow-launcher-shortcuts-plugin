using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface ICommandsService
{
    List<Result> ResolveCommand(List<string> arguments, Query query);
}