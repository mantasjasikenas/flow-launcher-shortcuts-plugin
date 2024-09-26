#nullable enable
using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface IShortcutsRepository : IAsyncInitializable
{
    IList<Shortcut> GetShortcuts();
    IEnumerable<Shortcut> GetPossibleShortcuts(string key);
    IList<Shortcut>? GetShortcuts(string key);
    bool TryGetShortcuts(string key, out List<Shortcut> shortcuts);
    void AddShortcut(Shortcut shortcut);
    void RemoveShortcut(Shortcut shortcut);
    void DuplicateShortcut(Shortcut shortcut, string duplicateKey);
    void ReloadShortcuts();
    void ImportShortcuts(string path);
    void ExportShortcuts(string path);
    IList<GroupShortcut> GetGroups();
    void GroupShortcuts(string groupKey, bool groupLaunch, IEnumerable<string> shortcutKeys);
}