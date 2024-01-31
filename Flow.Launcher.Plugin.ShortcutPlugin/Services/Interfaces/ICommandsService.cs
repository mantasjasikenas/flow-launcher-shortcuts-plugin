using System.Collections.Generic;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface ICommandsService
{
    List<Result> ResolveCommand([NotNull] List<string> arguments, Query query);

    void ReloadPluginData();
}