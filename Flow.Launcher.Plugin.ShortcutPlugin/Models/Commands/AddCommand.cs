using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class AddCommand : ICommand
{
    private readonly IShortcutsRepository _shortcutsRepository;

    public AddCommand(IShortcutsRepository shortcutsRepository)
    {
        _shortcutsRepository = shortcutsRepository;
    }

    public Command Create()
    {
        return CreateAddCommand();
    }

    private Command CreateAddCommand()
    {
        return new CommandBuilder()
               .WithKey("add")
               .WithResponseInfo(("add", "Add shortcuts to the list"))
               .WithResponseFailure(("Enter shortcut type", "Which type of shortcut do you want to add?"))
               .WithArguments(GetShortcutTypes())
               .Build();
    }

    private IEnumerable<IQueryExecutor> GetShortcutTypes()
    {
        return new List<IQueryExecutor>
        {
            CreateShortcutType("directory", CreateDirectoryShortcutHandler),
            CreateShortcutType("file", CreateFileShortcutHandler),
            CreateShortcutType("url", CreateUrlShortcutHandler),
            CreateShellShortcut(),
            CreateSnippetShortcut()
        };
    }

    private List<Result> CreateUrlShortcutHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;

        return ResultExtensions.SingleResult("Create url shortcut", $"Url: {arguments[3]}",
            () =>
            {
                var key = arguments[2];
                var url = arguments[3];

                _shortcutsRepository.AddShortcut(new UrlShortcut
                {
                    Key = key,
                    Url = url
                });
            });
    }

    private List<Result> CreateFileShortcutHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;

        return ResultExtensions.SingleResult("Create file shortcut", $"File path: {arguments[3]}", () =>
        {
            var key = arguments[2];
            var filePath = arguments[3];

            _shortcutsRepository.AddShortcut(new FileShortcut
            {
                Key = key,
                Path = filePath
            });
        });
    }

    private List<Result> CreateDirectoryShortcutHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;

        return ResultExtensions.SingleResult("Create directory shortcut", $"Directory path: {arguments[3]}",
            () =>
            {
                var key = arguments[2];
                var directoryPath = arguments[3];

                _shortcutsRepository.AddShortcut(new DirectoryShortcut
                {
                    Key = key,
                    Path = directoryPath
                });
            });
    }

    private List<Result> CreateShellShortcutHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;

        if (arguments.Count < 6)
        {
            return ResultExtensions.SingleResult("Invalid shell shortcut arguments",
                "Please provide valid shell shortcut arguments");
        }

        if (!Enum.TryParse<ShellType>(arguments[2], true, out var shellType))
        {
            return ResultExtensions.SingleResult("Invalid shell type",
                "Please provide valid shell type (cmd/powershell)");
        }

        if (!bool.TryParse(arguments[4], out var silent))
        {
            return ResultExtensions.SingleResult("Invalid silent argument",
                "Please provide valid silent argument (true/false)");
        }

        var key = arguments[3];
        var shellArguments = string.Join(" ", arguments.Skip(5));

        var subtitles = new List<string>
        {
            $"Type: {shellType.ToString().ToLower()}",
            $"key: {key}",
            $"silent: {silent.ToString().ToLower()}",
            $"command: {shellArguments}"
        };
        var subtitle = string.Join(", ", subtitles);

        return ResultExtensions.SingleResult("Create shell shortcut", subtitle, () =>
        {
            _shortcutsRepository.AddShortcut(new ShellShortcut
            {
                Key = key,
                ShellType = shellType,
                Silent = silent,
                Arguments = shellArguments
            });
        });
    }

    private static IQueryExecutor CreateShortcutType(string type,
        Func<ActionContext, ParsedQuery, List<Result>> createShortcutHandler)
    {
        var createShortcutHandlerArgument = new ArgumentBuilder()
                                            .WithResponseSuccess(("Add", "Your new shortcut will be added to the list"))
                                            .WithResponseInfo(("Enter shortcut path",
                                                "This is where your shortcut will point to"))
                                            .WithHandler(createShortcutHandler)
                                            .Build();

        var shortcutNameArgument = new ArgumentBuilder()
                                   .WithResponseFailure(("Enter shortcut path",
                                       "This is where your shortcut will point to"))
                                   .WithResponseInfo(("Enter shortcut name", "How should your shortcut be named?"))
                                   .WithArgument(createShortcutHandlerArgument)
                                   .Build();

        return new ArgumentLiteralBuilder()
               .WithKey(type)
               .WithResponseFailure(("Enter shortcut name", "How should your shortcut be named?"))
               .WithResponseInfo(($"add {type}", ""))
               .WithArgument(shortcutNameArgument)
               .Build();
    }

    private IQueryExecutor CreateShellShortcut()
    {
        var shellArgumentsArgument = new ArgumentBuilder()
                                     .WithResponseInfo(
                                         ("Enter shell arguments", "What should your shell arguments be?"))
                                     .WithHandler(CreateShellShortcutHandler)
                                     .WithMultipleValuesForSingleArgument()
                                     .Build();

        var silentExecutionArgument = new ArgumentBuilder()
                                      .WithResponseInfo(("Should execution be silent?",
                                          "Should execution be silent? (true/false)"))
                                      .WithArgument(shellArgumentsArgument)
                                      .Build();

        var shortcutNameArgument = new ArgumentBuilder()
                                   .WithResponseInfo(("Enter shortcut name", "How should your shortcut be named?"))
                                   .WithArgument(silentExecutionArgument)
                                   .Build();

        var shellTypeArgument = new ArgumentBuilder()
                                .WithResponseInfo(("Enter shell type",
                                    "Which shell should be used? (cmd/powershell/pwsh)"))
                                .WithArgument(shortcutNameArgument)
                                .Build();

        return new ArgumentLiteralBuilder()
               .WithKey("shell")
               .WithResponseInfo(("add shell", ""))
               .WithArgument(shellTypeArgument)
               .Build();
    }

    private IQueryExecutor CreateSnippetShortcut()
    {
        var snippetValueArgument = new ArgumentBuilder()
                                   .WithResponseInfo(("Enter snippet value", "What should the snippet value be?"))
                                   .WithHandler(CreateSnippetShortcutHandler)
                                   .Build();

        var shortcutNameArgument = new ArgumentBuilder()
                                   .WithResponseInfo(("Enter shortcut name", "How should your shortcut be named?"))
                                   .WithArgument(snippetValueArgument)
                                   .Build();

        return new ArgumentLiteralBuilder()
               .WithKey("snippet")
               .WithResponseInfo(("add snippet", ""))
               .WithArgument(shortcutNameArgument)
               .Build();
    }

    private List<Result> CreateSnippetShortcutHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;

        if (arguments.Count < 4)
        {
            return ResultExtensions.SingleResult("Invalid snippet shortcut arguments",
                "Please provide valid snippet shortcut arguments");
        }

        var key = arguments[2];
        var value = arguments[3];

        return ResultExtensions.SingleResult("Create snippet shortcut", $"Snippet: {value}", () =>
        {
            _shortcutsRepository.AddShortcut(new SnippetShortcut
            {
                Key = key,
                Value = value
            });
        });
    }
}