using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    [ObservableProperty]
    private ElementTheme _currentTheme;

    [ObservableProperty]
    private string _versionDescription;

    private int _themeIndex;

    public int ThemeIndex
    {
        get => _themeIndex;
        set
        {
            if (_themeIndex == value)
            {
                return;
            }

            _themeIndex = value;
            SwitchThemeCommand.Execute((ElementTheme)value);
            OnPropertyChanged();
        }
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _currentTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        ThemeIndex = (int)_themeSelectorService.Theme;
    }

    [RelayCommand]
    private async Task SwitchTheme(ElementTheme theme)
    {
        if (CurrentTheme == theme)
        {
            return;
        }

        CurrentTheme = theme;
        await _themeSelectorService.SetThemeAsync(theme);
    }


    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
