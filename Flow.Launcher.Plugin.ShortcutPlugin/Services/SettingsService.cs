using System;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class SettingsService : ISettingsService
{
    private readonly IPluginManager _pluginManager;

    private Settings _settings;

    private readonly Settings _defaultSettings;

    public SettingsService(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        _defaultSettings = GetDefaultSettings();

        LoadSettings();

        MigrateSettings();
    }

    public void Reload()
    {
        LoadSettings();
    }

    public T GetSettingOrDefault<T>(Func<Settings, T> getAction)
    {
        var value = getAction(_settings);

        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return getAction(_defaultSettings);
        }

        return value;
    }

    public T GetDefaultSetting<T>(Func<Settings, T> getAction)
    {
        return getAction(_defaultSettings);
    }

    public T GetSetting<T>(Func<Settings, T> getAction)
    {
        return getAction(_settings);
    }

    public void SetSettings<T>(Action<Settings, T> setAction, T value)
    {
        setAction(_settings, value);
        SaveSettings();
    }

    public void Reset()
    {
        _settings.ShortcutsPath = string.Empty;
        _settings.VariablesPath = string.Empty;

        SaveSettings();
    }

    private void MigrateSettings()
    {
        var modified = false;

        var shortcutPluginDirectory = _pluginManager.Metadata.PluginDirectory;
        var pluginsDirectory = Directory.GetParent(shortcutPluginDirectory)?.Parent?.FullName;

        if (pluginsDirectory is null)
        {
            return;
        }

        // default paths are empty strings
        var shortcutsPath = _settings.ShortcutsPath;
        var variablesPath = _settings.VariablesPath;

        if (!string.IsNullOrEmpty(shortcutsPath) &&
            Path.GetFullPath(shortcutsPath).StartsWith(pluginsDirectory))
        {
            _settings.ShortcutsPath = string.Empty;
            modified = true;
        }

        if (!string.IsNullOrEmpty(variablesPath) &&
            Path.GetFullPath(variablesPath).StartsWith(pluginsDirectory))
        {
            _settings.VariablesPath = string.Empty;
            modified = true;
        }

        if (!modified)
        {
            return;
        }

        SaveSettings();

        _pluginManager.API.ShowMsg("Settings paths have been migrated to the new location.",
            "Settings have been migrated because you where using the default settings path. This should fix the issue when shortcuts disappear after updating the plugin.");
        _pluginManager.API.ReloadAllPluginData();
    }

    private void SaveSettings()
    {
        _pluginManager.API.SaveSettingJsonStorage<Settings>();
    }

    private void LoadSettings()
    {
        _settings = _pluginManager.API.LoadSettingJsonStorage<Settings>();
    }

    private Settings GetDefaultSettings()
    {
        var pluginDirectory = _pluginManager.Metadata.PluginDirectory;
        var parentDirectory = Directory.GetParent(pluginDirectory)?.Parent?.FullName;

        if (parentDirectory is null)
        {
            return new Settings();
        }

        var path = Path.Combine(parentDirectory, Constants.PluginDataPath);

        return new Settings
        {
            ShortcutsPath = Path.Combine(path, Constants.ShortcutsFileName),
            VariablesPath = Path.Combine(path, Constants.VariablesFileName)
        };
    }

    /*
    private void LoadDefaultSettings(ICollection<string> invalidProperties)
    {
        var defaultSettings = GetDefaultSettings();

        if (invalidProperties.Contains(nameof(Settings.ShortcutsPath)))
        {
            _settings.ShortcutsPath = defaultSettings.ShortcutsPath;
        }

        if (invalidProperties.Contains(nameof(Settings.VariablesPath)))
        {
            _settings.VariablesPath = defaultSettings.VariablesPath;
        }

        SaveSettings();
    }

    private static (bool, List<string>) Validate(Settings settings)
    {
        var invalidProperties = new List<string>();

        if (settings is null)
        {
            return (false, invalidProperties);
        }

        if (string.IsNullOrWhiteSpace(settings.ShortcutsPath))
        {
            invalidProperties.Add(nameof(settings.ShortcutsPath));
        }

        if (string.IsNullOrWhiteSpace(settings.VariablesPath))
        {
            invalidProperties.Add(nameof(settings.VariablesPath));
        }

        return (invalidProperties.Count == 0, invalidProperties);
    }*/
}