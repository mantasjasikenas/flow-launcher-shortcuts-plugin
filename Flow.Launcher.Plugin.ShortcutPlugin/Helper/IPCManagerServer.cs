using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

// ReSharper disable once InconsistentNaming
public class IPCManagerServer
{
    private readonly Action<string> _logMessage;
    private readonly IReloadable _reloadable;

    public IPCManagerServer(Action<string> logMessage, IReloadable reloadable)
    {
        _logMessage = logMessage;
        _reloadable = reloadable;
    }

    public async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var pipeServer = new NamedPipeServerStream(Constants.ShortcutsPluginPipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message);

            try
            {
                await pipeServer.WaitForConnectionAsync(cancellationToken);
                using var reader = new StreamReader(pipeServer, Encoding.UTF8);
                var message = await reader.ReadLineAsync(cancellationToken);

                if (message == null)
                {
                    continue;
                }
                
                HandleMessage(message);
            }
            catch (Exception ex)
            {
                _logMessage($"Error: {ex.Message}");
            }
        }
    }

    private void HandleMessage(string message)
    {
        if (message == IPCCommand.ReloadPluginData.ToString())
        {
            _reloadable.ReloadData();
            return;
        }

        _logMessage($"Received message: {message}");
    }
}

