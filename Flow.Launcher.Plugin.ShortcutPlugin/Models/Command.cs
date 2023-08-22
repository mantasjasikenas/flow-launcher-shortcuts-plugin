using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.models;

public class Command : BaseQueryExecutor
{
}

public class Argument : BaseQueryExecutor
{
}

public class ArgumentLiteral : Argument
{
}

public class BaseQueryExecutor : IQueryExecutor
{
    public List<IQueryExecutor> Arguments { get; init; }

    public Func<ActionContext, List<string>, List<Result>> Handler { get; init; }

    [NotNull] public string Key { get; init; }

    public (string, string)? ResponseFailure { get; init; }

    public (string, string) ResponseInfo { get; init; }

    public (string, string)? ResponseSuccess { get; init; }
}

public interface IQueryExecutor : IArgumentsHolder
{
    public Func<ActionContext, List<string>, List<Result>> Handler { get; init; }

    public (string, string)? ResponseFailure { get; init; }

    public (string, string) ResponseInfo { get; init; }

    public (string, string)? ResponseSuccess { get; init; }
}

public interface IArgumentsHolder
{
    public List<IQueryExecutor> Arguments { get; init; }
}