using System.Text.Json;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;

public static class VariableUtilities
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public static async Task<Dictionary<string, Variable>> ReadVariables(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            await using var openStream = File.OpenRead(path);
            var variables = await JsonSerializer.DeserializeAsync<List<Variable>>(openStream);

            return variables.ToDictionary(variable => variable.Name);
        }
        catch (Exception)
        {
            return [];
        }
    }

    public static void SaveVariables(List<Variable> variables, string variablesPath)
    {
        var json = JsonSerializer.Serialize(variables, WriteOptions);

        File.WriteAllText(variablesPath, json);
    }

    public static string GetVariablesPath(string shortcutsPluginPath)
    {
        var settingsPath = SettingsUtilities.GetSettingsFilePath(shortcutsPluginPath);

        if (string.IsNullOrEmpty(settingsPath))
        {
            return string.Empty;
        }

        var settings = SettingsUtilities.ReadSettings(settingsPath);

        if (settings is not null && !string.IsNullOrEmpty(settings.VariablesPath))
        {
            return settings.VariablesPath;
        }

        var defaultSettings = SettingsUtilities.GetDefaultSettings(shortcutsPluginPath);

        return defaultSettings.VariablesPath;
    }
}