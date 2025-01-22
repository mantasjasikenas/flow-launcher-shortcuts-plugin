using System.IO.Pipes;
using System.Text;
using Helper = Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;

public class IPCManagerClient
{
    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            using var pipeClient = new NamedPipeClientStream(".", Helper.Constants.ShortcutsPluginPipeName, PipeDirection.Out);
            await pipeClient.ConnectAsync(5_000, cancellationToken);

            using var writer = new StreamWriter(pipeClient, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteLineAsync(message.AsMemory(), cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
