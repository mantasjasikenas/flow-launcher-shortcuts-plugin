namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;

public interface INavigationAware
{
    Task OnNavigatedTo(object parameter);

    Task OnNavigatedFrom();
}
