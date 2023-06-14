using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class ShortcutsService : IShortcutsService
{
    private readonly IShortcutsRepository _shortcutsRepository;

    public ShortcutsService(IShortcutsRepository shortcutsRepository)
    {
        _shortcutsRepository = shortcutsRepository;
    }

    public List<Result> GetShortcuts()
    {
        var shortcuts = _shortcutsRepository.GetShortcuts();

        if (shortcuts.Count == 0)
            return ResultExtensions.EmptyResult();

        return shortcuts.Select(shortcut => new Result
                        {
                            Title = $"{shortcut.Key.ToUpper()}",
                            SubTitle = $"{shortcut.Path}",
                            IcoPath = "images\\icon.png",
                            Action = _ =>
                            {
                                FileUtility.OpenShortcut(shortcut);
                                return true;
                            }
                        })
                        .ToList();
    }

    public List<Result> AddShortcut(string key, string path, ShortcutType type)
    {
        if (type is ShortcutType.Unknown)
            type = PathExtensions.ResolveShortcutType(path);

        var title = string.Format(Resources.ShortcutsManager_AddShortcut_Add_shortcut,
            type is ShortcutType.Unknown
                ? ""
                : $"{type.ToString().ToLower()}" + " type",
            key);

        return ResultExtensions.SingleResult(
            title, path.RetrieveFullPath(),
            () =>
            {
                _shortcutsRepository.AddShortcut(new Shortcut
                {
                    Key = key,
                    Path = path,
                    Type = type
                });
            });
    }

    public List<Result> RemoveShortcut(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
            return ResultExtensions.EmptyResult("Shortcut not found.");

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_RemoveShortcut_Remove_shortcut, key.ToUpper()), shortcut.Path,
            () => { _shortcutsRepository.RemoveShortcut(key); });
    }

    public List<Result> GetShortcutPath(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
            return ResultExtensions.EmptyResult("Shortcut not found.");

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_GetShortcutPath_Copy_shortcut_path, key.ToUpper()), shortcut.Path,
            () => { Clipboard.SetText(shortcut.Path); });
    }

    public List<Result> ChangeShortcutPath(string key, string path)
    {
        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_ChangeShortcutPath_Change_shortcut_path, key.ToUpper()), path,
            () => { _shortcutsRepository.ReplaceShortcutPath(key, path); });
    }

    public List<Result> OpenShortcut(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
            return ResultExtensions.EmptyResult();

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_OpenShortcut_Open_shortcut, shortcut.Key.ToUpper()),
            shortcut.Path,
            () => { FileUtility.OpenShortcut(shortcut); });
    }

    public  List<Result> DuplicateShortcut(string key, string newKey)
    {
        if(_shortcutsRepository.GetShortcut(key) is null)
            return ResultExtensions.EmptyResult($"Shortcut '{key}' not found.");

        return ResultExtensions.SingleResult(
            $"Duplicate shortcut '{key}' to '{newKey}'",
            "",
            () => { _shortcutsRepository.DuplicateShortcut(key, newKey); });
    }

    public List<Result> ImportShortcuts()
    {
        return ResultExtensions.SingleResult(Resources.Import_shortcuts, "",
            () => { _shortcutsRepository.ImportShortcuts(); });
    }

    public List<Result> ExportShortcuts()
    {
        return ResultExtensions.SingleResult(Resources.Export_shortcuts, "",
            () => { _shortcutsRepository.ExportShortcuts(); });
    }

    public void Reload()
    {
        _shortcutsRepository.ReloadShortcuts();
    }
}