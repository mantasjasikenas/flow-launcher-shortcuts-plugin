using System.Collections.Generic;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface ICommandsService
{
    bool TryInvokeCommand(string query, out List<Result> results);

    List<Result> ResolveCommand([NotNull] List<string> arguments);

    void ReloadPluginData();
}