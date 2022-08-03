namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class Helper
    {
        public string Keyword { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }

        public Helper(string keyword, string description, string example)
        {
            Keyword = keyword;
            Description = description;
            Example = example;
        }
    }
}