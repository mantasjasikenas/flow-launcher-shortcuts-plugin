using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public interface IShortcutsService
{
    List<Shortcut> GetShortcuts();
    List<Result> AddShortcut(string key, string path);
    List<Result> RemoveShortcut(string key, string path);
    List<Result> GetShortcutPath(string key, string path);
    List<Result> ChangeShortcutPath(string key, string path);
    List<Result> OpenShortcut(string key);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> ListShortcuts();
    void Reload();
}