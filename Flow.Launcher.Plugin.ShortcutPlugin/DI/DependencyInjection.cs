using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
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
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IShortcutsRepository, ShortcutsRepository>();
        services.AddSingleton<IShortcutsService, ShortcutsService>();
        services.AddSingleton<IVariablesRepository, VariablesRepository>();
        services.AddSingleton<ICommandsRepository, CommandsRepository>();
        services.AddSingleton<IVariablesService, VariablesService>();
        services.AddSingleton<ICommandsService, CommandsService>();
        services.AddSingleton<ISettingProvider, ShortcutPlugin>();
        services.AddSingleton<IShortcutHandler, ShortcutHandler>();
        services.AddSingleton<SettingsViewModel, SettingsViewModel>();
        services.AddScoped<ContextMenu>();

        return services;
    }

    public static IServiceCollection RegisterCommands(this IServiceCollection services)
    {
        services.AddSingleton<ICommand, AddCommand>();
        services.AddSingleton<ICommand, ListCommand>();
        services.AddSingleton<ICommand, ReloadCommand>();
        services.AddSingleton<ICommand, SettingsCommand>();
        services.AddSingleton<ICommand, ConfigCommand>();
        services.AddSingleton<ICommand, ImportCommand>();
        services.AddSingleton<ICommand, ExportCommand>();
        services.AddSingleton<ICommand, VariablesCommand>();
        services.AddSingleton<ICommand, RemoveCommand>();
        services.AddSingleton<ICommand, DuplicateCommand>();
        services.AddSingleton<ICommand, KeywordCommand>();
        services.AddSingleton<ICommand, GroupCommand>();
        services.AddSingleton<ICommand, ReportCommand>();
        services.AddSingleton<ICommand, VersionCommand>();

        return services;
    }
}