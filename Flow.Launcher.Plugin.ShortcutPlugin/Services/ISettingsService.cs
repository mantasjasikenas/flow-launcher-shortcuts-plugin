using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public interface ISettingsService
{
    Dictionary<string, Func<string, string, List<Result>>> Settings { get; }
    Dictionary<string, Func<List<Result>>> Commands { get; }
}