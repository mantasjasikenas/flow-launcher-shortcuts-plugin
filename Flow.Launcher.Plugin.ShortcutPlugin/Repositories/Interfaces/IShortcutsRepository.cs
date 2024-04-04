using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface IShortcutsRepository
{
    IList<Shortcut> GetShortcuts();
    IEnumerable<Shortcut> GetPossibleShortcuts(string query);
    [CanBeNull] IList<Shortcut> GetShortcuts(string key);
    void AddShortcut(Shortcut shortcut);
    void RemoveShortcut(Shortcut shortcut);
    void DuplicateShortcut(Shortcut shortcut, string duplicateKey);
    void ReloadShortcuts();
    void ImportShortcuts(string path);
    void ExportShortcuts(string path);
    IList<GroupShortcut> GetGroups();
    void GroupShortcuts(string groupKey, IEnumerable<string> shortcutKeys);
}