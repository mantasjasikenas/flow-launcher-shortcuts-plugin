using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IShortcutsService
{
    List<Result> RemoveShortcut(string key);
    List<Result> RemoveGroup(string key);
    List<Result> OpenShortcuts(IList<Shortcut> shortcuts, IEnumerable<string> arguments, bool expandGroups);
    IEnumerable<Result> OpenShortcut(Shortcut shortcut, IEnumerable<string> arguments, bool expandGroups);
    List<Result> DuplicateShortcut(string existingKey, string newKey);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> GetShortcuts(List<string> arguments, ShortcutType? shortcutType = null);
    List<Result> GetGroups();
    void Reload();
}