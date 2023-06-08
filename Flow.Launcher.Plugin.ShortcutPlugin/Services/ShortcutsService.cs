﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

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

    public Dictionary<string, Shortcut> GetShortcuts()
    {
        return _shortcutsRepository.GetShortcuts();
    }

    public List<Result> AddShortcut(string shortcut, string path)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_AddShortcut_Add_shortcut, shortcut.ToUpper()), path,
            () => { _shortcutsRepository.AddShortcut(shortcut, path); });
    }

    public List<Result> RemoveShortcut(string shortcut, string path)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_RemoveShortcut_Remove_shortcut, shortcut.ToUpper()), path,
            () => { _shortcutsRepository.RemoveShortcut(shortcut); });
    }

    public List<Result> GetShortcutPath(string shortcut, string path)
    {
        if (_shortcutsRepository.GetShortcuts().TryGetValue(shortcut, out var shortcutPath))
            path = shortcutPath.Path;

        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_GetShortcutPath_Copy_shortcut_path, shortcut.ToUpper()), path,
            () => { Clipboard.SetText(path); });
    }

    public List<Result> ChangeShortcutPath(string shortcut, string path)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_ChangeShortcutPath_Change_shortcut_path, shortcut.ToUpper()), path,
            () => { _shortcutsRepository.ChangeShortcutPath(shortcut, path); });
    }

    public List<Result> OpenShortcut(string shortcut)
    {
        return Utils.Utils.SingleResult(
            string.Format(Resources.ShortcutsManager_OpenShortcut_Open_shortcut, shortcut.ToUpper()),
            _shortcutsRepository.GetShortcuts()[shortcut].Path,
            () => { Utils.Utils.OpenShortcut(_shortcutsRepository.GetShortcuts()[shortcut]); });
    }

    public List<Result> ImportShortcuts()
    {
        return Utils.Utils.SingleResult(Resources.Import_shortcuts, "",
            () => { _shortcutsRepository.ImportShortcuts(); });
    }

    public List<Result> ExportShortcuts()
    {
        return Utils.Utils.SingleResult(Resources.Export_shortcuts, "",
            () => { _shortcutsRepository.ExportShortcuts(); });
    }

    public List<Result> ListShortcuts()
    {
        return _shortcutsRepository
               .GetShortcuts()
               .Select(shortcut => new Result
               {
                   Title = $"{shortcut.Key.ToUpper()}",
                   SubTitle = $"{shortcut.Value.Path}",
                   IcoPath = "images\\icon.png",
                   Action = _ =>
                   {
                       Utils.Utils.OpenShortcut(shortcut.Value);
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

    public static List<Result> DefaultResult()
    {
        return Utils.Utils.SingleResult(Resources.ShortcutsManager_Init_Plugin_initialized);
    }
}