using System;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface ISettingsService
{
    void ModifySettings(Action<Settings> modifyAction);
    T GetSetting<T>(Func<Settings, T> getAction);
    void SetSettings<T>(Action<Settings, T> setAction, T value);
    Settings GetSettings();
    void Reset();
    void Reload();
}