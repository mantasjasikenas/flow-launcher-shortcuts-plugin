using System;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface ISettingsService
{
    T GetSettingOrDefault<T>(Func<Settings, T> getAction);

    T GetSetting<T>(Func<Settings, T> getAction);

    T GetDefaultSetting<T>(Func<Settings, T> getAction);

    void SetSettings<T>(Action<Settings, T> setAction, T value);

    void Reset();

    void Reload();
}