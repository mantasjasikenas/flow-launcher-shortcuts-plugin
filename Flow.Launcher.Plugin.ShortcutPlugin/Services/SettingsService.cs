using System;
using System.IO;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class SettingsService : ISettingsService
{
    private readonly PluginInitContext _context;
    private Settings _settings;

    public SettingsService(PluginInitContext context)
    {
        _context = context;
        _settings = LoadSettings();
    }

    public void SaveSettings()
    {
        _context.API.SaveSettingJsonStorage<Settings>();
    }

    public Settings LoadSettings()
    {
        var settings = _context.API.LoadSettingJsonStorage<Settings>();

        if (settings is {ShortcutsPath: null})
        {
            LoadDefaultSettings();
        }


        return settings;
    }

    public void Reload()
    {
        _settings = LoadSettings();
    }

    private void LoadDefaultSettings()
    {
        _settings.ShortcutsPath =
            Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, Constants.ShortcutsFileName);

        SaveSettings();
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
}