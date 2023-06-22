using System.Diagnostics;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Validators;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Handlers;

public class PathHandler : IPathHandler
{
    private readonly IVariablesService _variablesService;

    public PathHandler(IVariablesService variablesService)
    {
        _variablesService = variablesService;
    }

    public ShortcutType ResolveShortcutType(string rawPath)
    {
        var path = _variablesService.ExpandVariables(rawPath);

        if (PathValidator.IsValidFile(path)) return ShortcutType.File;

        if (PathValidator.IsValidDirectory(path)) return ShortcutType.Directory;

        return PathValidator.IsValidUrl(path) ? ShortcutType.Url : ShortcutType.Unspecified;
    }

    public void OpenShortcut(Shortcut shortcut)
    {
        var path = _variablesService.ExpandVariables(shortcut.Path);

        switch (shortcut.Type)
        {
            case ShortcutType.Directory:
                OpenDirectory(path);
                break;
            case ShortcutType.File:
                OpenFile(path);
                break;
            case ShortcutType.Url:
                OpenUrl(path);
                break;
            default:
                return;
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