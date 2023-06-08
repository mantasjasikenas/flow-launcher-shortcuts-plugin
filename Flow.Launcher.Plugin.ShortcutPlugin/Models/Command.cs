namespace Flow.Launcher.Plugin.ShortcutPlugin.models;

public class Command
{
    public string Keyword { get; private set; }
    public string Id { get; private set; }
    public string Path { get; private set; }


    public static CommandBuilder Builder()
    {
        return new CommandBuilder();
    }

    public class CommandBuilder
    {
        private readonly Command _command = new();

        public CommandBuilder SetKeyword(string keyword)
        {
            _command.Keyword = keyword;
            return this;
        }

        public CommandBuilder SetId(string id)
        {
            _command.Id = id;
            return this;
        }

        public CommandBuilder SetPath(string path)
        {
            _command.Path = path;
            return this;
        }

        public Command Build()
        {
            return _command;
        }
    }
}