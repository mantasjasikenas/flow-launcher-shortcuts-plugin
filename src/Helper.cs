namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class Helper
    {
        public string Keyword { get; }
        public string Description { get; }
        public string Example { get; }

        public Helper(string keyword, string description, string example)
        {
            Keyword = keyword;
            Description = description;
            Example = example;
        }
    }
}