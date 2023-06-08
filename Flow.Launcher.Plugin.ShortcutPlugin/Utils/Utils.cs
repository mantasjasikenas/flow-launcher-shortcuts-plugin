using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Command = Flow.Launcher.Plugin.ShortcutPlugin.models.Command;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utils;

public static partial class Utils
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
        /*Cli.Wrap("explorer.exe")
           .WithArguments(path)
           .ExecuteAsync();*/

        Process.Start("explorer.exe", path);
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


    private static bool IsValidFile(string path) =>
        File.Exists(path);


    private static bool IsValidDirectory(string path) =>
        Directory.Exists(path);

    private static bool IsValidUrl(string path) =>
        Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute);


    public static Command Split(string query)
    {
        try
        {
            var keyword = KeywordRegex().Match(query).ToString();
            var id = IdRegex().Matches(query)[1].ToString();
            var path = PathRegex().Match(query).ToString();

            return Command
                   .Builder()
                   .SetId(id)
                   .SetPath(path)
                   .SetKeyword(keyword)
                   .Build();
        }
        catch
        {
            return null;
        }
    }

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

        if(IsValidUrl(path))
        {
            return ShortcutType.Url;
        }

        return ShortcutType.Unknown;
    }

    // Returns a list with a single result
    public static List<Result> SingleResult(string title, string subtitle = "", Action action = default,
        bool hideAfterAction = true)
    {
        return new List<Result>
        {
            new()
            {
                Title = title,
                SubTitle = subtitle,
                IcoPath = "images\\icon.png",
                Action = _ =>
                {
                    action?.Invoke();
                    return hideAfterAction;
                }
            }
        };
    }

    [GeneratedRegex("^\\w+")]
    private static partial Regex KeywordRegex();

    [GeneratedRegex("\\w+")]
    private static partial Regex IdRegex();

    [GeneratedRegex("(?<=\\w+ \\w+ ).+(?<=\\n|$)")]
    private static partial Regex PathRegex();
}