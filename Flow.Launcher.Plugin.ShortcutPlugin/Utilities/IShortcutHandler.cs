using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

public interface IShortcutHandler
{
    void ExecuteShortcut(Shortcut shortcut, List<string> arguments);
}