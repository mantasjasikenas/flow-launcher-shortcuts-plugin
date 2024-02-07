using CliWrap;
using CliWrap.Buffered;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Extensions;

public static class Extensions
{
    public static CommandResult Execute(this Command command)
    {
        return command.ExecuteAsync().GetAwaiter().GetResult();
    }

    public static BufferedCommandResult ExecuteBuffered(this Command command)
    {
        return command.ExecuteBufferedAsync().GetAwaiter().GetResult();
    }
}
