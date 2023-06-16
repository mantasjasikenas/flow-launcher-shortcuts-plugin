using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IShortcutsService
{
    List<Result> AddShortcut(string key, string path, ShortcutType type);
    List<Result> RemoveShortcut(string key);
    List<Result> GetShortcutPath(string key);
    List<Result> ChangeShortcutPath(string key, string path);
    List<Result> OpenShortcut(string key);
    List<Result> DuplicateShortcut(string key, string newKey);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> GetShortcuts();
    void Reload();
}