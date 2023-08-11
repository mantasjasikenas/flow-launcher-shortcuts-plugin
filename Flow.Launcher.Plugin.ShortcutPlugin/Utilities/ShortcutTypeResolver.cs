using System.Diagnostics;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Validators;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

public class ShortcutTypeResolver : IShortcutTypeResolver
{
    private readonly IVariablesService _variablesService;

    public ShortcutTypeResolver(IVariablesService variablesService)
    {
        _variablesService = variablesService;
    }

    // TODO refactor this method / move to a different class
    public ShortcutType ResolveType(string path)
    {
        if (PathValidator.IsValidFile(path)) return ShortcutType.File;

        if (PathValidator.IsValidDirectory(path)) return ShortcutType.Directory;

        return PathValidator.IsValidUrl(path) ? ShortcutType.Url : ShortcutType.Unspecified;
    }

    // TODO refactor this method / move to a different class
    public void ExecuteShortcut(Shortcut shortcut)
    {
        switch (shortcut)
        {
            case PluginShortcut:
            {
                // handle PluginShortcut
                break;
            }
            case ProgramShortcut:
            {
                // handle ProgramShortcut
                break;
            }
            case UrlShortcut urlShortcut:
            {
                var path = _variablesService.ExpandVariables(urlShortcut.Url);
                OpenUrl(path);

                break;
            }
            case DirectoryShortcut directoryShortcut:
            {
                var path = _variablesService.ExpandVariables(directoryShortcut.Path);
                OpenDirectory(path);

                break;
            }
            case FileShortcut fileShortcut:
            {
                var path = _variablesService.ExpandVariables(fileShortcut.Path);
                OpenFile(path);

                break;
            }
        }
    }

    private static void OpenFile(string path)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };
        Process.Start(processStartInfo);
    }


    private static void OpenDirectory(string path)
    {
        Cli.Wrap("explorer.exe")
           .WithArguments(path)
           .ExecuteAsync();
    }

    private static void OpenUrl(string url)
    {
        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = url
        };
        Process.Start(processStartInfo);
    }
}