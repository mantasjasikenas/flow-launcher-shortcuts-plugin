using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using CliWrap;
using Command = Flow.Launcher.Plugin.ShortcutPlugin.models.Command;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utils;

public static class Utils
{
    public static void OpenFileOrFolder(string path)
    {
        if (IsFile(path))
        {
            OpenFile(path);
        }
        else if (IsDirectory(path))
        {
            OpenFolder(path);
        }
        else
        {
            MessageBox.Show(string.Format(Resources.Utils_OpenFileOrFolder_File_or_directory_does_not_exist, path));
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


    private static void OpenFolder(string path)
    {
        Cli.Wrap("explorer.exe")
           .WithArguments(path)
           .ExecuteAsync();
    }


    private static bool IsFile(string path) =>
        File.Exists(path);


    private static bool IsDirectory(string path) =>
        Directory.Exists(path);


    public static Command Split(string query)
    {
        try
        {
            var keyword = Regex.Match(query, @"^\w+").ToString();
            var id = Regex.Matches(query, @"\w+")[1].ToString();
            var path = Regex.Match(query, @"(?<=\w+ \w+ ).+(?<=\n|$)").ToString();

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

    // Returns a list with a single result
    public static List<Result> SingleResult(string title, string subtitle = "", Action action = default,
        bool hideAfterAction = true)
    {
        return new List<Result>
        {
            new Result
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
}