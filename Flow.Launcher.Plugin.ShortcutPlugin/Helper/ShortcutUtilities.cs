using System.Diagnostics;
using CliWrap;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public static class ShortcutUtilities
{
    public static void OpenPowershell(string arguments, bool silent)
    {
        if (silent)
        {
            Cli.Wrap("powershell.exe").WithArguments(arguments).ExecuteAsync();
            return;
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoExit -Command \"{arguments}\"",
            UseShellExecute = true,
            CreateNoWindow = true
        };
        Process.Start(processStartInfo);
    }

    public static void OpenPwsh(string arguments, bool silent)
    {
        if (silent)
        {
            Cli.Wrap("pwsh.exe").WithArguments(arguments).ExecuteAsync();
            return;
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "pwsh.exe",
            Arguments = $"-NoExit -Command \"{arguments}\"",
            UseShellExecute = true,
            CreateNoWindow = true
        };
        Process.Start(processStartInfo);
    }

    public static void OpenCmd(string arguments, bool silent)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            CreateNoWindow = silent
        };

        if (silent)
        {
            processStartInfo.Arguments = "/c " + "\"" + arguments + "\"";
        }
        else
        {
            processStartInfo.Arguments = "/K " + "\"" + arguments + "\"";
            processStartInfo.UseShellExecute = true;
        }

        Process.Start(processStartInfo);
    }

    public static void OpenFile(string path, string? app = null)
    {
        ProcessStartInfo processStartInfo;

        if (!string.IsNullOrEmpty(app))
        {
            processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = app,
                Arguments = path
            };
            Process.Start(processStartInfo);
            return;
        }

        processStartInfo = new ProcessStartInfo {FileName = path, UseShellExecute = true};
        Process.Start(processStartInfo);
    }

    public static void OpenDirectory(string path)
    {
        Cli.Wrap("explorer.exe").WithArguments(path).ExecuteAsync();
    }

    public static void OpenUrl(string url, string? app = null)
    {
        ProcessStartInfo processStartInfo;

        if (!string.IsNullOrEmpty(app))
        {
            processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = app,
                Arguments = url.Replace(" ", "%20")
            };
            Process.Start(processStartInfo);
            return;
        }

        processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = url.Replace(" ", "%20")
        };
        Process.Start(processStartInfo);
    }
}