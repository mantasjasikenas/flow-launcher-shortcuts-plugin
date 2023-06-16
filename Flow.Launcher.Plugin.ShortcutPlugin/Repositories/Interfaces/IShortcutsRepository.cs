using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface IShortcutsRepository
{
    Shortcut GetShortcut(string id);
    IList<Shortcut> GetShortcuts();
    void AddShortcut(Shortcut shortcut);
    void AddShortcut(string id, string shortcutPath);
    void RemoveShortcut(string id);
    void ReplaceShortcut(Shortcut shortcut);
    void ReplaceShortcutPath(string id, string shortcutPath);
    void DuplicateShortcut(string key, string duplicateKey);
    void ReloadShortcuts();
    void ImportShortcuts();
    void ExportShortcuts();
}