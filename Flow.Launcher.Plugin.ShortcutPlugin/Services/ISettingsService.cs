using System;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public interface ISettingsService
{
    void SaveSettings();
    Settings LoadSettings();
    void Reload();
    void ModifySettings(Action<Settings> modifyAction);
    T GetSetting<T>(Func<Settings, T> getAction);
    void SetSettings<T>(Action<Settings, T> setAction, T value);
}