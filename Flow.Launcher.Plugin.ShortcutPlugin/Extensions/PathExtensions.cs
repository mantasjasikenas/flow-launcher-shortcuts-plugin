using System;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Extensions;

public static class PathExtensions
{
    public static bool IsValidFile(string path) =>
        File.Exists(path);

    public static bool IsValidDirectory(string path) =>
        Directory.Exists(path);

    public static bool IsValidUrl(string path) =>
        Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);

    public static ShortcutType ResolveShortcutType(string path)
    {
        if (IsValidFile(path))
        {
            return ShortcutType.File;
        }

        if (IsValidDirectory(path))
        {
            return ShortcutType.Directory;
        }

        return IsValidUrl(path) ? ShortcutType.Url : ShortcutType.Unknown;
    }
}