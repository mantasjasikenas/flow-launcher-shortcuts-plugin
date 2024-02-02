using System;
using System.Collections.Generic;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class SettingsService : ISettingsService
{
    private readonly PluginInitContext _context;
    private Settings _settings;

    public SettingsService(PluginInitContext context)
    {
        _context = context;
        LoadSettings();
    }

    public Settings GetSettings()
    {
        return _settings;
    }

    public void Reload()
    {
        LoadSettings();
    }

    public void ModifySettings(Action<Settings> modifyAction)
    {
        modifyAction(_settings);
        SaveSettings();
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

    private void SaveSettings()
    {
        _context.API.SaveSettingJsonStorage<Settings>();
    }

    private void LoadSettings()
    {
        _settings = _context.API.LoadSettingJsonStorage<Settings>();
        var (isValid, invalidProperties) = Validate(_settings);

        if (!isValid)
        {
            LoadDefaultSettings(invalidProperties);
        }
    }

    private void LoadDefaultSettings(ICollection<string> invalidProperties)
    {
        if (invalidProperties.Contains(nameof(Settings.ShortcutsPath)))
        {
            _settings.ShortcutsPath =
                Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, Constants.ShortcutsFileName);
        }

        if (invalidProperties.Contains(nameof(Settings.VariablesPath)))
        {
            _settings.VariablesPath =
             Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, Constants.VariablesFileName);
        }


        SaveSettings();
    }

    private static (bool, List<string>) Validate(Settings settings)
    {
        var invalidProperties = new List<string>();

        if (settings.ShortcutsPath is null)
        {
            invalidProperties.Add(nameof(settings.ShortcutsPath));
        }

        if (settings.VariablesPath is null)
        {
            invalidProperties.Add(nameof(settings.VariablesPath));
        }

        return (invalidProperties.Count == 0, invalidProperties);
    }
}