using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public static class Utils
    {

        public static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show(string.Format(Resources.Utils_OpenFolder_Directory_does_not_exist, folderPath));
            }
        }

        public static Command Split(string query)
        {
            Command command = null;
            try
            {
                var keyword = Regex.Match(query, @"^\w+").ToString();
                var id = Regex.Matches(query, @"\w+")[1].ToString();
                var path = Regex.Match(query, @"(?<=\w+ \w+ ).+(?<=\n|$)").ToString();

                command = new Command(keyword, id, path);
            }
            catch
            {
                // ignored
            }

            return command;
        }
        
        // Returns a list with a single result
        public static List<Result> SingleResult(string title, string subtitle = "", Action action = default, bool hideAfterAction = true) =>
            new()
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