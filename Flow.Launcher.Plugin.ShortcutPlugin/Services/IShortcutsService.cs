using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public interface IShortcutsService
{
    Dictionary<string, Shortcut> GetShortcuts();
    List<Result> AddShortcut(string shortcut, string path);
    List<Result> RemoveShortcut(string shortcut, string path);
    List<Result> GetShortcutPath(string shortcut, string path);
    List<Result> ChangeShortcutPath(string shortcut, string path);
    List<Result> OpenShortcut(string shortcut);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> ListShortcuts();
    void Reload();
}