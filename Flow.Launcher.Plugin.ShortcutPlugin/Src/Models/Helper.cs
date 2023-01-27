namespace Flow.Launcher.Plugin.ShortcutPlugin.models;

public class Helper
{
    public string Keyword { get;  set; }
    public string Description { get;  set; }
    public string Example { get;  set; }


    public static HelperBuilder Builder()
    {
        return new HelperBuilder();
    }

    public class HelperBuilder
    {
        private readonly Helper _helper = new();

        public HelperBuilder SetKeyword(string keyword)
        {
            _helper.Keyword = keyword;
            return this;
        }

        public HelperBuilder SetDescription(string description)
        {
            _helper.Description = description;
            return this;
        }

        public HelperBuilder SetExample(string example)
        {
            _helper.Example = example;
            return this;
        }

        public Helper Build()
        {
            return _helper;
        }
    }
}