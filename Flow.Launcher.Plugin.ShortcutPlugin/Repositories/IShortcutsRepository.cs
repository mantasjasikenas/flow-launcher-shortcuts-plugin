using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public interface IShortcutsRepository
{
    void ReloadShortcuts();
    Dictionary<string, Shortcut> GetShortcuts();
    void AddShortcut(string id, string shortcutPath);
    void RemoveShortcut(string id);
    void ChangeShortcutPath(string id, string shortcutPath);
    void ImportShortcuts();
    void ExportShortcuts();
}