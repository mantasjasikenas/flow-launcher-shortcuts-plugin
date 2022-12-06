namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class Command
    {
        public string Keyword { get; }
        public string Id { get; }
        public string Path { get; }

        public Command(string keyword, string id, string path)
        {
            Keyword = keyword;
            Id = id;
            Path = path;
        }
    }
}