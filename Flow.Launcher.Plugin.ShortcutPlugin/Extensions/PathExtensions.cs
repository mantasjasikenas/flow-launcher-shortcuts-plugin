using System;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Extensions;

public static class PathExtensions
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

    public static ShortcutType ResolveShortcutType(string path)
    {
        path = path.RetrieveFullPath();

        if (IsValidFile(path)) return ShortcutType.File;

        if (IsValidDirectory(path)) return ShortcutType.Directory;

        return IsValidUrl(path) ? ShortcutType.Url : ShortcutType.Unknown;
    }

    public static string RetrieveFullPath(this string path)
    {
        return Environment.ExpandEnvironmentVariables(path);
    }
}