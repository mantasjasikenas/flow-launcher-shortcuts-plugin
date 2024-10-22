using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly IShortcutsService _shortcutsService;
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

    [ObservableProperty]
    private string shortcutsPath = string.Empty;

    [ObservableProperty]
    private string variablesPath = string.Empty;

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService, IShortcutsService shortcutsService)
    {
        _themeSelectorService = themeSelectorService;
        _localSettingsService = localSettingsService;
        _shortcutsService = shortcutsService;
        _currentTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        ThemeIndex = (int)_themeSelectorService.Theme;

    }

    public async Task OnNavigatedTo(object parameter)
    {
        var shortcutsPath = await _localSettingsService.ReadSettingAsync<string>(Constants.ShortcutPathKey);

        if (!string.IsNullOrEmpty(shortcutsPath))
        {
            ShortcutsPath = shortcutsPath;
        }

        var variablesPath = await _localSettingsService.ReadSettingAsync<string>(Constants.VariablesPathKey);

        if (!string.IsNullOrEmpty(variablesPath))
        {
            VariablesPath = variablesPath;
        }
    }
    public Task OnNavigatedFrom()
    {
        return Task.CompletedTask;
    }

    public async Task SetShortcutsPath(string path)
    {
        ShortcutsPath = path;

        await _localSettingsService.SaveSettingAsync(Constants.ShortcutPathKey, path);

        await _shortcutsService.RefreshShortcutsAsync();
    }

    public async Task SetVariablesPath(string path)
    {
        VariablesPath = path;

        await _localSettingsService.SaveSettingAsync(Constants.VariablesPathKey, path);

        // TODO Refresh variables
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
