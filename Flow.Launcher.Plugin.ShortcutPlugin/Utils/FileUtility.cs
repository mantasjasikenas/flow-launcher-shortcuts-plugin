    using System.Diagnostics;
    using CliWrap;
    using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utils;

public static class FileUtility
{
    public static void OpenShortcut(Shortcut shortcut)
    {
        var path = shortcut.Path;

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

    private static void OpenFile(string path)  {
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

        //Process.Start("explorer.exe", path);
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