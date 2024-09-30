namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
