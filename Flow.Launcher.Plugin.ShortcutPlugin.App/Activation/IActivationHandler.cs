namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}
