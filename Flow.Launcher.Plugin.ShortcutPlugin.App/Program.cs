using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App;

public class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            _ = AsyncMain();
        }
        catch (Exception)
        {

        }
    }

    private static async Task AsyncMain()
    {
        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            var isRedirect = await DecideRedirection();

            if (!isRedirect)
            {
                Application.Start((_) =>
                 {
                     DispatcherQueueSynchronizationContext context = new(
                         DispatcherQueue.GetForCurrentThread());
                     SynchronizationContext.SetSynchronizationContext(context);
                     var app = new App();
                 });
            }
        }
        catch (Exception)
        {

        }
    }

    private static async Task<bool> DecideRedirection()
    {
        try
        {
            var isRedirect = false;

            var keyInstance = AppInstance.FindOrRegisterForKey("Mantelis.ShortcutsApp.MainInterface");

            if (keyInstance.IsCurrent)
            {
                keyInstance.Activated += async (_, e) =>
                {
                    if (App.Current is App baseInstance)
                    {
                        await baseInstance.ShowMainWindowFromRedirectAsync(e);
                    }
                };
            }
            else
            {
                isRedirect = true;
                var args = AppInstance.GetCurrent().GetActivatedEventArgs();

                await keyInstance.RedirectActivationToAsync(args);
            }
            return isRedirect;
        }
        catch (Exception)
        {
            return false;
        }
    }
}