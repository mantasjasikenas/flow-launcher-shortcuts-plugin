using System;
using System.IO;

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
}