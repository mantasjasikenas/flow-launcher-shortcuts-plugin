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
    private readonly string _pluginDirectory;
    private IShortcutsRepository _shortcutsRepository;

    public ShortcutsService(string pluginDirectory, IShortcutsRepository shortcutsRepository)
    {
        _pluginDirectory = pluginDirectory;
        _shortcutsRepository = shortcutsRepository;
    }

    public List<Shortcut> GetShortcuts()
    {
        return _shortcutsRepository.GetShortcuts().ToList();
    }

    public List<Result> AddShortcut(string key, string path)
    {
        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_AddShortcut_Add_shortcut, key.ToUpper()), path,
            () => { _shortcutsRepository.AddShortcut(key, path); });
    }

    public List<Result> RemoveShortcut(string key, string path)
    {
        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_RemoveShortcut_Remove_shortcut, key.ToUpper()), path,
            () => { _shortcutsRepository.RemoveShortcut(key); });
    }

    public List<Result> GetShortcutPath(string key, string path)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is not null)
            path = shortcut.Path;

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_GetShortcutPath_Copy_shortcut_path, key.ToUpper()), path,
            () => { Clipboard.SetText(path); });
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

    public List<Result> ListShortcuts()
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

    public void Reload()
    {
        if (_shortcutsRepository is null)
            _shortcutsRepository = new ShortcutsRepository(_pluginDirectory);
        else
            _shortcutsRepository.ReloadShortcuts();
    }
}