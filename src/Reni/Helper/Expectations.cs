#nullable enable
using System.Diagnostics;
using hw.Scanner;

namespace Reni.Helper;

static class Expectations
{
    internal enum BreakModeType { UseAssert, UseException }

    [PublicAPI]
    internal sealed class ExpectationFailedException : Exception
    {
        [PublicAPI]
        internal readonly SourcePart? Position;

        public ExpectationFailedException(string cond, (SourcePart? position, string? message)? data)
            : base(cond + (data?.message == null? "" : " " + data.Value.message))
            => Position = data?.position;
    }

    internal static BreakModeType BreakMode;

    [PublicAPI]
    [DebuggerHidden]
    [ContractAnnotation("target: null => halt")]
    public static TResult ExpectNotNull<TResult>
        (this TResult? target, Func<(SourcePart?, string?)>? getData = null, int stackFrameDepth = 0)
        where TResult : class
    {
        target.ExpectIsNotNull(getData, stackFrameDepth: stackFrameDepth + 1);
        return target!;
    }


    [PublicAPI]
    [DebuggerHidden]
    [ContractAnnotation("b: null => halt")]
    internal static void ExpectIsNotNull
        (this object? b, Func<(SourcePart?, string?)>? getData = null, int stackFrameDepth = 0)
    {
        if(b != null)
            return;
        ExpectationFailed("ExpectIsNotNull", getData, stackFrameDepth + 1);
    }

    [PublicAPI]
    [DebuggerHidden]
    [ContractAnnotation("b: notnull => halt")]
    internal static void ExpectIsNull(this object? b, Func<(SourcePart?, string?)> getData, int stackFrameDepth = 0)
    {
        if(b == null)
            return;
        ExpectationFailed($"ExpectIsNull: {b.LogDump()}", getData, stackFrameDepth + 1);
    }

    [DebuggerHidden]
    [ContractAnnotation("b: false => halt")]
    public static void Expect(this bool b, Func<(SourcePart?, string?)>? getData= null, int stackFrameDepth = 0)
    {
        if(b)
            return;
        ExpectationFailed("ExpectTrue", getData, stackFrameDepth + 1);
    }

    [PublicAPI]
    [DebuggerHidden]
    internal static void Expect<TTargetType>
        (this object target, Func<(SourcePart?, string?)> getData, int stackFrameDepth = 0)
    {
        if(target is TTargetType)
            return;
        ExpectationFailed($"Expect is {typeof(TTargetType).PrettyName()}", getData, stackFrameDepth + 1);
    }

    [DebuggerHidden]
    [IsLoggingFunction]
    static void ExpectationFailed
        (string cond, Func<(SourcePart? position, string? message)>? getData = null, int stackFrameDepth = 0)
    {
        if(BreakMode == BreakModeType.UseAssert)
        {
            var data = getData?.Invoke();
            if(data == null)
                Tracer.AssertionFailed(cond, stackFrameDepth: stackFrameDepth + 1);
            else
            {
                var (position, message) = data.Value;
                var text = $"{position?.Id}: {message} ";
                Tracer.AssertionFailed(cond, () => text, stackFrameDepth + 1);
            }

            return;
        }

        var stackFrame = new StackTrace(true).GetFrame(stackFrameDepth + 1);
        throw new ExpectationFailedException(cond, getData?.Invoke());
    }
}