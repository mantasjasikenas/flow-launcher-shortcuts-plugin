using System.Collections.Generic;


namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public interface ICommandsService
{
    bool TryInvokeCommand(string query, out List<Result> results);

    void ReloadPluginData();
}