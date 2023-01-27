using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Manager;

public class ShortcutsManager
{
    private ShortcutsStorage _shortcutsStorage;
    private readonly string _pluginDirectory;


    public ShortcutsManager(string pluginDirectory)
    {
        _pluginDirectory = pluginDirectory;
        _shortcutsStorage = new ShortcutsStorage(pluginDirectory);
    }

    public Dictionary<string, string> GetShortcuts()
    {
        return _shortcutsStorage.Shortcuts;
    }

    public static List<Result> Init()
    {
        return Utils.Utils.SingleResult(Resources.ShortcutsManager_Init_Plugin_initialized);
    }

    public List<Result> AddShortcut(string shortcut, string path)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_AddShortcut_Add_shortcut, shortcut.ToUpper()), path,
            () => { _shortcutsStorage.AddShortcut(shortcut, path); });
    }

    public List<Result> RemoveShortcut(string shortcut, string path)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_RemoveShortcut_Remove_shortcut, shortcut.ToUpper()), path,
            () => { _shortcutsStorage.RemoveShortcut(shortcut); });
    }

    public List<Result> GetShortcutPath(string shortcut, string path)
    {
        if (_shortcutsStorage.Shortcuts.ContainsKey(shortcut))
            path = _shortcutsStorage.Shortcuts[shortcut];

        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_GetShortcutPath_Copy_shortcut_path, shortcut.ToUpper()), path,
            () => { Clipboard.SetText(path); });
    }

    public List<Result> ChangeShortcutPath(string shortcut, string path)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_ChangeShortcutPath_Change_shortcut_path, shortcut.ToUpper()), path,
            () => { _shortcutsStorage.ChangeShortcutPath(shortcut, path); });
    }

    public List<Result> OpenShortcut(string shortcut)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_OpenShortcut_Open_shortcut, shortcut.ToUpper()),
            _shortcutsStorage.Shortcuts[shortcut],
            () => { Utils.Utils.OpenFolder(_shortcutsStorage.Shortcuts[shortcut]); });
    }

    public List<Result> ImportShortcuts()
    {
        return Utils.Utils.SingleResult(Resources.Import_shortcuts, "",
            () => { _shortcutsStorage.ImportShortcuts(); });
    }

    public List<Result> ExportShortcuts()
    {
        return Utils.Utils.SingleResult(Resources.Export_shortcuts, "",
            () => { _shortcutsStorage.ExportShortcuts(); });
    }

    public List<Result> ListShortcuts()
    {
        return _shortcutsStorage.Shortcuts.Select(shortcut => new Result
                                {
                                    Title = $"{shortcut.Key.ToUpper()}",
                                    SubTitle = $"{shortcut.Value}",
                                    IcoPath = "images\\icon.png",
                                    Action = _ =>
                                    {
                                        Utils.Utils.OpenFolder(shortcut.Value);
                                        return true;
                                    }
                                })
                                .ToList();
    }

    public void Reload()
    {
        if (_shortcutsStorage is null)
            _shortcutsStorage = new ShortcutsStorage(_pluginDirectory);
        else
            _shortcutsStorage.ReloadShortcuts();
    }
}