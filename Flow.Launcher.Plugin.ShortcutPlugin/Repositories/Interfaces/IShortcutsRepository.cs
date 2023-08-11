using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface IShortcutsRepository
{
    IList<Shortcut> GetShortcuts();
    Shortcut GetShortcut(string key);
    void AddShortcut(Shortcut shortcut);
    void RemoveShortcut(string key);
    void ReplaceShortcut(Shortcut shortcut);
    void DuplicateShortcut(string existingKey, string duplicateKey);
    void ReloadShortcuts();
    void ImportShortcuts(string path);
    void ExportShortcuts(string path);
}