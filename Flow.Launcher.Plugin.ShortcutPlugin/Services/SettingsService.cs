﻿using System;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class SettingsService : ISettingsService
{
    private readonly PluginInitContext _context;

    private Settings _settings;

    private readonly Settings _defaultSettings;

    public SettingsService(PluginInitContext context)
    {
        _context = context;
        _defaultSettings = GetDefaultSettings();

        LoadSettings();
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

    private void SaveSettings()
    {
        _context.API.SaveSettingJsonStorage<Settings>();
    }

    private void LoadSettings()
    {
        _settings = _context.API.LoadSettingJsonStorage<Settings>();
    }

    private Settings GetDefaultSettings()
    {
        var pluginDirectory = _context.CurrentPluginMetadata.PluginDirectory;
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