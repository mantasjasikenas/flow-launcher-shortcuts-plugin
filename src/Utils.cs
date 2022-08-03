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
                MessageBox.Show($"{folderPath} Directory does not exist!");
            }
        }

        public static Command Split(string text)
        {
            Command command = null;
            try
            {
                var keyword = Regex.Match(text, @"^\w+").ToString();
                var id = Regex.Matches(text, @"\w+")[1].ToString();
                var path = Regex.Match(text, @"(?<=\w+ \w+ ).+(?<=\n|$)").ToString();

                command = new Command(keyword, id, path);
            }
            catch
            {
                // ignored
            }

            return command;
        }
    }
}