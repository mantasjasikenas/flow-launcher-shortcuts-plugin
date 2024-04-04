using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IShortcutsService
{
    List<Result> RemoveShortcut(string key);
    List<Result> RemoveGroup(string key);
    List<Result> OpenShortcuts(string key, IEnumerable<string> arguments);
    List<Result> OpenShortcut(Shortcut shortcut, IEnumerable<string> arguments);
    List<Result> DuplicateShortcut(string existingKey, string newKey);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> GetShortcuts(List<string> arguments);
    List<Result> GetGroups();
    void Reload();
}