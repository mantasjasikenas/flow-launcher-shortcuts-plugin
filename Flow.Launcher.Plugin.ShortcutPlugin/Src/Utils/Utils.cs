using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using CliWrap;
using Command = Flow.Launcher.Plugin.ShortcutPlugin.models.Command;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utils;

public static class Utils
{
    public static void OpenFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            MessageBox.Show(string.Format(Resources.Utils_OpenFolder_Directory_does_not_exist, folderPath));
            return;
        }

        Cli.Wrap("explorer.exe")
           .WithArguments(folderPath)
           .ExecuteAsync();
    }


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