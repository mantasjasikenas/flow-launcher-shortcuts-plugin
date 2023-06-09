using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public interface ICommandsService
{
    Dictionary<string, Func<string, string, List<Result>>> CommandsWithParams { get; }
    Dictionary<string, Func<List<Result>>> CommandsWithoutParams { get; }

    bool TryInvokeCommand(string query, out List<Result> results);
}