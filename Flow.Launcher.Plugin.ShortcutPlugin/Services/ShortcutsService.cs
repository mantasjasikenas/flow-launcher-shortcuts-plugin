using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class ShortcutsService : IShortcutsService
{
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IShortcutTypeResolver _shortcutTypeResolver;
    private readonly IVariablesService _variablesService;


    public ShortcutsService(IShortcutsRepository shortcutsRepository,
        IShortcutTypeResolver shortcutTypeResolver,
        IVariablesService variablesService)
    {
        _shortcutsRepository = shortcutsRepository;
        _shortcutTypeResolver = shortcutTypeResolver;
        _variablesService = variablesService;
    }

    public List<Result> GetShortcuts()
    {
        var shortcuts = _shortcutsRepository.GetShortcuts();

        if (shortcuts.Count == 0)
            return ResultExtensions.EmptyResult();

        return shortcuts.Select(shortcut =>
                        {
                            return new Result
                            {
                                Title = $"{shortcut.Key.ToUpper()}",
                                SubTitle = $"{shortcut} ({shortcut.GetDerivedType()})",
                                IcoPath = "images\\icon.png",
                                Action = _ =>
                                {
                                    _shortcutTypeResolver.ExecuteShortcut(shortcut);
                                    return true;
                                }
                            };
                        })
                        .ToList();
    }

    public List<Result> AddShortcut(string key, string rawPath, ShortcutType type)
    {
        var path = _variablesService.ExpandVariables(rawPath);

        if (type is ShortcutType.Unspecified)
            type = _shortcutTypeResolver.ResolveType(path);

        var title = string.Format(Resources.ShortcutsManager_AddShortcut_Add_shortcut,
            type is ShortcutType.Unspecified
                ? ""
                : $"{type.ToString().ToLower()}" + " type",
            key);

        Shortcut shortcut = type switch
        {
            ShortcutType.Directory => new DirectoryShortcut
            {
                Key = key,
                Path = path
            },
            ShortcutType.File => new FileShortcut
            {
                Key = key,
                Path = path
            },
            ShortcutType.Url => new UrlShortcut
            {
                Key = key,
                Url = path
            },
            ShortcutType.Plugin => new PluginShortcut
            {
                Key = key
            },
            ShortcutType.Program => new ProgramShortcut
            {
                Key = key
            },
            _ => null
        };

        if (shortcut is null)
            return ResultExtensions.EmptyResult("Shortcut type not supported.");

        return ResultExtensions.SingleResult(
            title, path,
            () => { _shortcutsRepository.AddShortcut(shortcut); });
    }

    public List<Result> RemoveShortcut(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
            return ResultExtensions.EmptyResult("Shortcut not found.");

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_RemoveShortcut_Remove_shortcut, key.ToUpper()),
            shortcut.ToString(),
            () => { _shortcutsRepository.RemoveShortcut(key); });
    }

    public List<Result> GetShortcutDetails(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
            return ResultExtensions.EmptyResult("Shortcut not found.");

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_GetShortcutPath_Copy_shortcut_path, key.ToUpper()),
            shortcut.ToString(),
            () =>
            {
                var details = shortcut.ToString();
                if (!string.IsNullOrEmpty(details))
                    Clipboard.SetText(details);
            });
    }

    public List<Result> OpenShortcut(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
            return ResultExtensions.EmptyResult();

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_OpenShortcut_Open_shortcut, shortcut.Key.ToUpper()),
            shortcut.ToString(),
            () => { _shortcutTypeResolver.ExecuteShortcut(shortcut); });
    }

    public List<Result> DuplicateShortcut(string key, string newKey)
    {
        if (_shortcutsRepository.GetShortcut(key) is null)
            return ResultExtensions.EmptyResult($"Shortcut '{key}' not found.");

        return ResultExtensions.SingleResult(
            $"Duplicate shortcut '{key}' to '{newKey}'",
            "",
            () => { _shortcutsRepository.DuplicateShortcut(key, newKey); });
    }

    public List<Result> ImportShortcuts()
    {
        return ResultExtensions.SingleResult(Resources.Import_shortcuts, "",
            () =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Resources.Import_shortcuts,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "json",
                    Filter = "JSON (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() != true)
                    return;

                _shortcutsRepository.ImportShortcuts(openFileDialog.FileName);
            });
    }

    public List<Result> ExportShortcuts()
    {
        return ResultExtensions.SingleResult(Resources.Export_shortcuts, "",
            () =>
            {
                var dialog = new SaveFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Resources.Export_shortcuts,
                    FileName = "shortcuts.json",
                    CheckPathExists = true,
                    DefaultExt = "json",
                    Filter = "JSON (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (dialog.ShowDialog() != true) return;
                var exportPath = dialog.FileName;

                _shortcutsRepository.ExportShortcuts(exportPath);
            });
    }

    public void Reload()
    {
        _shortcutsRepository.ReloadShortcuts();
    }
}