using System.Text.RegularExpressions;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Activation;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Core.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Notifications;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App;

public partial class App : Application
{
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar
    {
        get; set;
    }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(ConfigureServices)
            .Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    public async Task ShowMainWindowFromRedirectAsync(AppActivationArguments rawArgs)
    {
        // keep in mind this is not my implementation
        while (MainWindow is null)
        {
            await Task.Delay(100);
        }

        MainWindow.DispatcherQueue.TryEnqueue(MainWindow.Activate);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Default Activation Handler
        services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

        // Other Activation Handlers
        services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

        // Services
        services.AddSingleton<IAppNotificationService, AppNotificationService>();
        services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
        services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
        services.AddTransient<INavigationViewService, NavigationViewService>();

        services.AddTransient<IPCManagerClient>();

        services.AddSingleton<IActivationService, ActivationService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // Core Services
        services.AddSingleton<IShortcutsService, ShortcutsService>();
        services.AddSingleton<IVariablesService, VariablesService>();
        services.AddSingleton<IFileService, FileService>();

        // Views and ViewModels
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<ShortcutsViewModel>();
        services.AddTransient<ShortcutsPage>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<HomePage>();
        services.AddTransient<ShellPage>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<ShortcutDetailsViewModel>();
        services.AddTransient<VariableDetailsViewModel>();
        services.AddTransient<VariablesViewModel>();

        // Configuration
        services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
