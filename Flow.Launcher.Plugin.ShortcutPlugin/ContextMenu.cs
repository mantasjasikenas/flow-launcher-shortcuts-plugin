using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;


namespace Flow.Launcher.Plugin.ShortcutPlugin;

internal class ContextMenu : IContextMenu
{
    // ReSharper disable once NotAccessedField.Local
    private readonly PluginInitContext _context;

    // ReSharper disable once NotAccessedField.Local
    private readonly IShortcutHandler _shortcutHandler;

    private readonly IVariablesService _variablesService;


    public ContextMenu(PluginInitContext context, IShortcutHandler shortcutHandler, IVariablesService variablesService)
    {
        _context = context;
        _shortcutHandler = shortcutHandler;
        _variablesService = variablesService;
    }

    public List<Result> LoadContextMenus(Result selectedResult)
    {
        var contextMenu = new List<Result>();

        if (selectedResult.ContextData is not Shortcut shortcut)
        {
            return contextMenu;
        }

        switch (shortcut)
        {
            case DirectoryShortcut directoryShortcut:
                GetDirectoryContextMenu(contextMenu, directoryShortcut);
                break;
            case FileShortcut fileShortcut:
                GetFileShortcutContextMenu(contextMenu, fileShortcut);
                break;
        }

        return contextMenu;
    }

    private void GetFileShortcutContextMenu(ICollection<Result> contextMenu, FileShortcut fileShortcut)
    {
        var filePath = _variablesService.ExpandVariables(fileShortcut.Path);

        contextMenu.Add(ResultExtensions.Result(
            "Open file",
            fileShortcut.Path,
            action: () =>
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            },
            iconPath: Icons.File
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Open containing folder",
            action: () =>
            {
                var path = Path.GetDirectoryName(filePath);

                if (path != null)
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    Process.Start(processStartInfo);
                }
            },
            iconPath: Icons.FolderOpen
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Copy path",
            action: () => { Clipboard.SetText(filePath); },
            iconPath: Icons.Copy
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Copy file",
            action: () => { Clipboard.SetFileDropList(new StringCollection {filePath}); },
            iconPath: Icons.Copy
        ));
    }

    private void GetDirectoryContextMenu(ICollection<Result> contextMenu, DirectoryShortcut directoryShortcut)
    {
        var directoryPath = _variablesService.ExpandVariables(directoryShortcut.Path);

        contextMenu.Add(ResultExtensions.Result(
            "Open in Explorer",
            directoryPath,
            action: () =>
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = directoryPath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            },
            iconPath: Icons.FolderOpen
        ));


        // open folder in vs code if it exists in the system
        if (TryGetVisualCode(out var command, out var icon))
        {
            contextMenu.Add(ResultExtensions.Result(
                "Open in VS Code",
                // directoryPath,
                action: () =>
                {
                    CliWrap.Cli.Wrap(command)
                           .WithArguments(".")
                           .WithWorkingDirectory(directoryPath)
                           .ExecuteAsync();
                },
                iconPath: icon
            ));
        }

        contextMenu.Add(ResultExtensions.Result(
            "Open in Command Prompt",
            // directoryPath,
            action: () =>
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k cd /d {directoryPath}",
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            },
            iconPath: Icons.WindowsTerminal
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Open in PowerShell",
            // directoryPath,
            action: () =>
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-noexit cd '{directoryPath}'",
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            },
            iconPath: Icons.PowerShell
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Copy path",
            action: () => { Clipboard.SetText(directoryPath); },
            iconPath: Icons.Copy
        ));

        var fileSystemEntries = Directory.GetFileSystemEntries(directoryPath);

        if (fileSystemEntries.Length == 0)
        {
            return;
        }

        contextMenu.Add(ResultExtensions.Result(
            "Directory Contents",
            "Files and folders in this directory",
            iconPath: Icons.Apps
        ));


        foreach (var path in fileSystemEntries)
        {
            var fileName = Path.GetFileName(path);
            var iconPath = Directory.Exists(path) ? Icons.Folder : Icons.File;

            contextMenu.Add(ResultExtensions.Result(
                fileName,
                path,
                action: () =>
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    Process.Start(processStartInfo);
                },
                iconPath: iconPath
            ));
        }
    }

    private static bool TryGetVisualCode(out string version, out string icon)
    {
        var path = Environment.GetEnvironmentVariable("PATH");

        version = null;
        icon = null;

        if (path == null)
        {
            return false;
        }

        var folders = path.Split(';');

        foreach (var folder in folders)
        {
            if (File.Exists(Path.Combine(folder, "code-insiders.cmd")))
            {
                version = "code-insiders";
                icon = Icons.VisualCodeInsiders;
                return true;
            }

            if (File.Exists(Path.Combine(folder, "code.cmd")))
            {
                version = "code";
                icon = Icons.VisualCode;
                return true;
            }
        }

        return false;
    }
}