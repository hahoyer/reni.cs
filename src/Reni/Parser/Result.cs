using Reni.Validation;

namespace Reni.Parser;

sealed class Result<TTarget> : DumpableObject
    where TTarget : class?
{
    internal TTarget Target { get; }
    internal Issue[]? Issues { get; }

    public Result(TTarget target, params Issue[]? issues)
    {
        Target = target;
        Issues = issues ?? [];
    }

    public static implicit operator Result<TTarget>(TTarget value)
        => new(value);

    public static Result<TTarget> From<TIn>(Result<TIn> x)
        where TIn : class, TTarget => new(x.Target, x.Issues);

    internal Result<TTarget> With(params Issue[]? issues) => new(Target, Issues.Plus(issues));

    internal Result<TOutTarget> Convert<TOutTarget>(Func<TTarget, Result<TOutTarget>> converter)
        where TOutTarget : class
    {
        var inner = converter(Target);
        return new(inner.Target, Issues.Plus(inner.Issues));
    }

    internal Result<TOutTarget> Convert<TOutTarget>(Func<TTarget, TOutTarget> converter)
        where TOutTarget : class
    {
        var inner = converter(Target);
        return new(inner, Issues);
    }
}