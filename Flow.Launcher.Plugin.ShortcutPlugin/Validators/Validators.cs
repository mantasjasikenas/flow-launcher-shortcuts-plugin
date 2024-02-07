using System;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Validators;

public static class Validators
{
    public static bool IsValidFile(string path)
    {
        return File.Exists(path);
    }

    public static bool IsValidDirectory(string path)
    {
        return Directory.Exists(path);
    }

    public static bool IsValidUrl(string path)
    {
        return Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);
    }

    public static bool IsValidShortcut(Shortcut shortcut)
    {
        if (shortcut is null)
        {
            return false;
        }

        switch (shortcut)
        {
            case UrlShortcut urlShortcut:
            {
                return IsValidUrl(urlShortcut.Url);
            }
            case DirectoryShortcut directoryShortcut:
            {
                return IsValidDirectory(directoryShortcut.Path);
            }
            case FileShortcut fileShortcut:
            {
                return IsValidFile(fileShortcut.Path);
            }
            default:
            {
                return false;
            }
        }
    }
}
