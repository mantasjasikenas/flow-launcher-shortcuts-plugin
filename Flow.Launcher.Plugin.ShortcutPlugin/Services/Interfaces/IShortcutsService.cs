using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IShortcutsService
{
    List<Result> AddShortcut(Shortcut shortcut);
    List<Result> RemoveShortcut(string key);
    List<Result> GetShortcutDetails(string key);
    List<Result> OpenShortcut(string key, IEnumerable<string> arguments);
    List<Result> DuplicateShortcut(string key, string newKey);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> GetShortcuts(List<string> arguments);
    void Reload();
    List<Result> GetGroups();
}