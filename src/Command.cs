namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class Command
    {
        public string Keyword { get; private set; }
        public string Id { get; private set; }
        public string Path { get; private set; }

        public Command(string keyword, string id, string path)
        {
            Keyword = keyword;
            Id = id;
            Path = path;
        }
        
        
    }
}