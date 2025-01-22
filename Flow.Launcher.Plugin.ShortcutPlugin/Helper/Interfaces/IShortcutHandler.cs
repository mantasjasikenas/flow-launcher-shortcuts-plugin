using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

public interface IShortcutHandler
{
    void ExecuteShortcut(Shortcut shortcut, IReadOnlyDictionary<string, string> arguments);
}