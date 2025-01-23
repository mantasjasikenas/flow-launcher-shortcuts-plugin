using System;
using System.IO;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public static class AppUtilities
{
    public static string GetApplicationPath(string appName)
    {
        if (Path.Exists(appName))
        {
            return appName;
        }

        if (!string.IsNullOrWhiteSpace(appName) && !appName.EndsWith(".exe"))
        {
            appName += ".exe";
        }

        using var key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "");
        using var subKey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + appName);

        var path = subKey?.GetValue("Path");

        if (path != null)
        {
            return (string) path + "\\" + appName;
        }

        return "";
    }

    
    // ReSharper disable once UnusedMember.Global
    public static string? GetSystemDefaultApp(string extension)
    {
        string? name;
        RegistryKey? regKey = null;

        try
        {
            var regDefault = Registry.CurrentUser.OpenSubKey(
                $"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{extension}\\UserChoice", false);
            var stringDefault = regDefault?.GetValue("ProgId");

            regKey = Registry.ClassesRoot.OpenSubKey(stringDefault + "\\shell\\open\\command", false);
            name = regKey?.GetValue(null)?.ToString()?.ToLower().Replace("" + (char) 34, "");

            if (name != null && !name.EndsWith("exe"))
            {
                name = name[..(name.LastIndexOf(".exe", StringComparison.Ordinal) + 4)];
            }
        }
        catch (Exception)
        {
            name = "";
        }
        finally
        {
            regKey?.Close();
        }

        return name;
    }

    public static string? GetSystemDefaultBrowser()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\http\UserChoice");

        var s = (string?) key?.GetValue("ProgId");

        using var command = Registry.ClassesRoot.OpenSubKey($"{s}\\shell\\open\\command");

        return (string?) command?.GetValue(null);
    }
}