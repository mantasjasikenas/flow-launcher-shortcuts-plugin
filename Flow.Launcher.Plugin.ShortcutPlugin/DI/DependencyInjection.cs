using Flow.Launcher.Plugin.ShortcutPlugin.Handlers;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin.DI;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, PluginInitContext context)
    {
        services.AddSingleton(context);
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IShortcutsRepository, ShortcutsRepository>();
        services.AddSingleton<IShortcutsService, ShortcutsService>();
        services.AddSingleton<IHelpersRepository, HelpersRepository>();
        services.AddSingleton<IVariablesRepository, VariablesRepository>();
        services.AddSingleton<IVariablesService, VariablesService>();
        services.AddSingleton<ICommandsService, CommandsService>();
        services.AddSingleton<ISettingProvider, ShortcutPlugin>();
        services.AddSingleton<IPathHandler, PathHandler>();


        return services;
    }
}