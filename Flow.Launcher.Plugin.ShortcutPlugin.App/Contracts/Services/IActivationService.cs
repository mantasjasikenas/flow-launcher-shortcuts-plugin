namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
