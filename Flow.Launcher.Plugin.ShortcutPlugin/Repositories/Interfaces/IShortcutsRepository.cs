using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface IShortcutsRepository : IAsyncInitializable
{
    IList<Shortcut> GetShortcuts(ShortcutType? shortcutType = null);
    IEnumerable<Shortcut> GetPossibleShortcuts(string key);
    IList<Shortcut>? GetShortcuts(string key);
    bool TryGetShortcuts(string key, [MaybeNullWhen(false)] out List<Shortcut> shortcuts);
    void AddShortcut(Shortcut shortcut);
    void RemoveShortcut(Shortcut shortcut);
    void DuplicateShortcut(Shortcut shortcut, string duplicateKey);
    IList<GroupShortcut> GetGroups();
    void GroupShortcuts(string groupKey, bool groupLaunch, IEnumerable<string> shortcutKeys);
    Task ReloadShortcutsAsync();
    Task ImportShortcuts(string path);
    void ExportShortcuts(string path);
}