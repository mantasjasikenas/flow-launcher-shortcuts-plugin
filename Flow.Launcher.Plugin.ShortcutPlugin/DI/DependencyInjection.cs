using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin.DI;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        PluginInitContext context
    )
    {
        services.AddSingleton(context);
        services.AddSingleton<IAsyncReloadable, Reloadable>();
        services.AddSingleton<IPluginManager, PluginManager>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IShortcutsRepository, ShortcutsRepository>();
        services.AddSingleton<IShortcutsService, ShortcutsService>();
        services.AddSingleton<IVariablesRepository, VariablesRepository>();
        services.AddSingleton<IBackupRepository, BackupRepository>();
        services.AddSingleton<ICommandsRepository, CommandsRepository>();
        services.AddSingleton<IVariablesService, VariablesService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<ISettingProvider, ShortcutPlugin>();
        services.AddSingleton<IShortcutHandler, ShortcutHandler>();
        services.AddSingleton<IIconProvider, IconProvider>();
        services.AddSingleton<IQueryParser, QueryParser>();
        services.AddSingleton<IQueryInterpreter, QueryInterpreter>();
        services.AddSingleton<SettingsViewModel, SettingsViewModel>();
        services.AddSingleton(provider => new IPCManagerServer(
            message =>
            {
                context.API.ShowMsg(message);
            },
            provider.GetService<IReloadable>()!
        ));
        services.AddScoped<ContextMenu>();

        return services;
    }

    public static IServiceCollection RegisterCommands(this IServiceCollection services)
    {
        services.AddSingleton<ICommand, AddCommand>();
        services.AddSingleton<ICommand, ListCommand>();
        services.AddSingleton<ICommand, ReloadCommand>();
        services.AddSingleton<ICommand, SnippetsCommand>();
        services.AddSingleton<ICommand, SettingsCommand>();
#if INCLUDE_EDITOR
        services.AddSingleton<ICommand, EditorCommand>();
#endif
        services.AddSingleton<ICommand, ConfigCommand>();
        services.AddSingleton<ICommand, ImportCommand>();
        services.AddSingleton<ICommand, ExportCommand>();
        services.AddSingleton<ICommand, BackupCommand>();
        services.AddSingleton<ICommand, VariablesCommand>();
        services.AddSingleton<ICommand, RemoveCommand>();
        services.AddSingleton<ICommand, DuplicateCommand>();
        services.AddSingleton<ICommand, KeywordCommand>();
        services.AddSingleton<ICommand, GroupCommand>();
        services.AddSingleton<ICommand, ReportCommand>();
        services.AddSingleton<ICommand, VersionCommand>();
        services.AddSingleton<ICommand, HelpCommand>();

        return services;
    }
}