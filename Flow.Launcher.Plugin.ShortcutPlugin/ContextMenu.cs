using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;


namespace Flow.Launcher.Plugin.ShortcutPlugin;

internal class ContextMenu : IContextMenu
{
    private readonly IVariablesService _variablesService;
    private readonly IPluginManager _pluginManager;

    public ContextMenu(IVariablesService variablesService, IPluginManager pluginManager)
    {
        _variablesService = variablesService;
        _pluginManager = pluginManager;
    }

    public List<Result> LoadContextMenus(Result selectedResult)
    {
        var contextMenu = new List<Result>();

        AddShortcutDetails(selectedResult, contextMenu);
        AddCopyTitleAndSubtitle(selectedResult, contextMenu);

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

    private void AddShortcutDetails(Result selectedResult, List<Result> contextMenu)
    {
        if (selectedResult.ContextData is not Shortcut shortcut)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(shortcut.Key))
        {
            contextMenu.Add(ResultExtensions.Result(
                "Key",
                shortcut.Key,
                () => { _pluginManager.API.CopyToClipboard(shortcut.Key, showDefaultNotification: false); }
            ));
        }

        if (shortcut.Alias is {Count: > 0})
        {
            contextMenu.Add(ResultExtensions.Result(
                "Alias",
                string.Join(", ", shortcut.Alias),
                () =>
                {
                    _pluginManager.API.CopyToClipboard(string.Join(", ", shortcut.Alias),
                        showDefaultNotification: false);
                }
            ));
        }

        if (!string.IsNullOrEmpty(shortcut.Description))
        {
            contextMenu.Add(ResultExtensions.Result(
                "Description",
                shortcut.Description,
                () => { _pluginManager.API.CopyToClipboard(shortcut.Description, showDefaultNotification: false); }
            ));
        }
    }

    private void AddCopyTitleAndSubtitle(Result selectedResult, List<Result> contextMenu)
    {
        var copyTitle = ResultExtensions.Result(
            "Copy result title",
            selectedResult.Title,
            action: () => { _pluginManager.API.CopyToClipboard(selectedResult.Title, showDefaultNotification: false); },
            iconPath: Icons.Copy
        );
        var copySubTitle = ResultExtensions.Result(
            "Copy result subtitle",
            selectedResult.SubTitle,
            action: () =>
            {
                _pluginManager.API.CopyToClipboard(selectedResult.SubTitle, showDefaultNotification: false);
            },
            iconPath: Icons.Copy
        );

        contextMenu.Add(copyTitle);
        contextMenu.Add(copySubTitle);
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
            Path.GetDirectoryName(filePath),
            action: () =>
            {
                var path = Path.GetDirectoryName(filePath);

                if (path == null)
                {
                    return;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            },
            iconPath: Icons.FolderOpen
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Copy path",
            filePath,
            action: () => { _pluginManager.API.CopyToClipboard(filePath, showDefaultNotification: false); },
            iconPath: Icons.Copy
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Copy file",
            action: () => { _pluginManager.API.CopyToClipboard(filePath, true, false); },
            iconPath: Icons.Copy
        ));

        var codeEditors = GetCodeEditors();

        foreach (var (title, cmd, icon) in codeEditors)
        {
            contextMenu.Add(ResultExtensions.Result(
                $"Open in {title}",
                action: () =>
                {
                    CliWrap.Cli.Wrap(cmd)
                           .WithArguments(filePath)
                           .ExecuteAsync();
                },
                iconPath: icon
            ));
        }
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

        var codeEditors = GetCodeEditors();

        foreach (var (title, cmd, icon) in codeEditors)
        {
            contextMenu.Add(ResultExtensions.Result(
                $"Open in {title}",
                action: () =>
                {
                    CliWrap.Cli.Wrap(cmd)
                           .WithArguments(".")
                           .WithWorkingDirectory(directoryPath)
                           .ExecuteAsync();
                },
                iconPath: icon
            ));
        }

        contextMenu.Add(ResultExtensions.Result(
            "Open in Command Prompt",
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
            iconPath: Icons.PowerShellBlack
        ));

        contextMenu.Add(ResultExtensions.Result(
            "Copy path",
            action: () => { _pluginManager.API.CopyToClipboard(directoryPath, showDefaultNotification: false); },
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

    private static List<(string title, string executable, string icon)> GetCodeEditors()
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        var versions = new List<(string, string, string)>();

        if (path == null)
        {
            return versions;
        }

        var editors = new Dictionary<string, (string title, string executable, string icon)>
        {
            {"code.cmd", ("Visual Studio Code", "code", Icons.VisualCode)},
            {"code-insiders.cmd", ("Visual Studio Code - Insiders", "code-insiders", Icons.VisualCodeInsiders)},
            {"zed.exe", ("Zed", "zed", Icons.Zed)}
        };

        versions.AddRange(
            path
                .Split(';')
                .SelectMany(_ => editors, (folder, editor) => new {folder, editor})
                .Where(t => File.Exists(Path.Combine(t.folder, t.editor.Key)))
                .Select(t => t.editor.Value)
        );

        return versions;
    }
}