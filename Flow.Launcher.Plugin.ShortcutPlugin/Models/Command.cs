using System;
using System.Collections.Generic;

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
    public string Key { get; set; }

    public List<IQueryExecutor> Arguments { get; set; }

    public bool AllowsMultipleValuesForSingleArgument { get; set; }

    public Func<ActionContext, List<string>, List<Result>> Handler { get; set; }

    public (string, string)? ResponseFailure { get; set; }

    public (string, string) ResponseInfo { get; set; }

    public (string, string)? ResponseSuccess { get; set; }
}

public interface IQueryExecutor
{
    public Func<ActionContext, List<string>, List<Result>> Handler { get; set; }

    public (string, string)? ResponseFailure { get; set; }

    public (string, string) ResponseInfo { get; set; }

    public (string, string)? ResponseSuccess { get; set; }

    public List<IQueryExecutor> Arguments { get; set; }

    public bool AllowsMultipleValuesForSingleArgument { get; set; }
}
