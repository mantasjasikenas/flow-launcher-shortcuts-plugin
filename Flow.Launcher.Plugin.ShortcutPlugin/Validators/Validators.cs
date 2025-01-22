using System;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Validators;

public static class Validators
{
    private static bool IsValidFile(string path)
    {
        return File.Exists(path);
    }

    private static bool IsValidDirectory(string path)
    {
        return Directory.Exists(path);
    }

    private static bool IsValidUrl(string path)
    {
        return Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);
    }

    public static bool ValidateShortcut(Shortcut shortcut)
    {
        if (shortcut is null)
        {
            return false;
        }

        return shortcut switch
        {
            UrlShortcut urlShortcut => IsValidUrl(urlShortcut.Url),
            DirectoryShortcut directoryShortcut => IsValidDirectory(directoryShortcut.Path),
            FileShortcut fileShortcut => IsValidFile(fileShortcut.Path),
            _ => false
        };
    }
}