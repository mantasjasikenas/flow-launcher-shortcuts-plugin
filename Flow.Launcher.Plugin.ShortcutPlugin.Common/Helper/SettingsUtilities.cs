using System.Text.Json;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;
using CommonContants = Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper.Constants;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
public static class SettingsUtilities
{
    /// <summary>
    /// Get the path of the settings file
    /// </summary>
    /// <param name="shortcutsPluginPath">Should point to plugin's root path
    /// For example: %AppData%\Roaming\FlowLauncher\Plugins\Shortcuts-1.2.1
    /// </param>
    /// <returns>Settings path</returns>
    public static string GetSettingsFilePath(string shortcutsPluginPath)
    {
        // C:\Users\tutta\AppData\Roaming\FlowLauncher
        var flowLauncherRootPath = Directory.GetParent(shortcutsPluginPath)?.Parent?.FullName;

        if (flowLauncherRootPath is null)
        {
            return string.Empty;
        }

        var pluginDataPath = Path.Combine(flowLauncherRootPath, CommonContants.PluginDataPath);
        var settingsPath = Path.Combine(pluginDataPath, CommonContants.SettingsFileName);

        return settingsPath;
    }

    public static Settings ReadSettings(string settingsPath)
    {
        var json = File.ReadAllText(settingsPath);

        return JsonSerializer.Deserialize<Settings>(json);
    }

    public static Settings GetDefaultSettings(string shortcutsPluginPath)
    {
        var settingsFilePath = GetSettingsFilePath(shortcutsPluginPath);
        var pluginDataPath = Path.GetDirectoryName(settingsFilePath);

        return new Settings
        {
            ShortcutsPath = Path.Combine(pluginDataPath, Constants.ShortcutsFileName),
            VariablesPath = Path.Combine(pluginDataPath, Constants.VariablesFileName)
        };
    }
}
