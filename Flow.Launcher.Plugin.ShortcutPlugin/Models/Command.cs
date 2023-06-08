using System;
using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.ShortcutPlugin.models;

public class Command : IParsable<Command>
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

    public static Command Parse(string s, IFormatProvider provider = null)
    {
        try
        {
            var keyword = Regex.Match(s, @"^\w+").ToString();
            var id = Regex.Matches(s, @"\w+")[1].ToString();
            var path = Regex.Match(s, @"(?<=\w+ \w+ ).+(?<=\n|$)").ToString();


            return Builder()
                   .SetId(id)
                   .SetPath(path)
                   .SetKeyword(keyword)
                   .Build();
        }
        catch
        {
            return null;
        }
    }

    public static bool TryParse(string s, IFormatProvider provider, out Command result)
    {
        result = Parse(s, provider);
        return result is not null;
    }
}