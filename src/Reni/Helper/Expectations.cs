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
		internal readonly SourcePart Position;

		[PublicAPI]
		internal readonly StackFrame StackFrame;

		public ExpectationFailedException(string cond, SourcePart position)
			: base(cond)
			=> Position = position;
	}

	internal static BreakModeType BreakMode;

	[PublicAPI]
	[DebuggerHidden]
	[ContractAnnotation("b: null => halt")]
	internal static void ExpectIsNotNull(this object b, Func<SourcePart> getSourcePart, int stackFrameDepth = 0)
	{
		if(b != null)
			return;
		ExpectationFailed("ExpectIsNotNull", getSourcePart, stackFrameDepth + 1);
	}

	[PublicAPI]
	[DebuggerHidden]
	[ContractAnnotation("b: notnull => halt")]
	internal static void ExpectIsNull(this object b, Func<SourcePart> getSourcePart, int stackFrameDepth = 0)
	{
		if(b == null)
			return;
		ExpectationFailed($"ExpectIsNull: {b.LogDump()}", getSourcePart, stackFrameDepth + 1);
	}

	[DebuggerHidden]
	[ContractAnnotation("b: false => halt")]
	public static void Expect(this bool b, Func<SourcePart> getSourcePart, int stackFrameDepth = 0)
	{
		if(b)
			return;
		ExpectationFailed("ExpectTrue", getSourcePart, stackFrameDepth + 1);
	}

	[PublicAPI]
	[DebuggerHidden]
	internal static void Expect<TTargetType>(this object target, Func<SourcePart> getSourcePart, int stackFrameDepth = 0)
	{
		if(target is TTargetType)
			return;
		ExpectationFailed($"Expect is {typeof(TTargetType).PrettyName()}", getSourcePart, stackFrameDepth + 1);
	}

	[DebuggerHidden]
	[IsLoggingFunction]
	static void ExpectationFailed(string cond, Func<SourcePart> getSourcePart = null, int stackFrameDepth = 0)
	{
		if(BreakMode == BreakModeType.UseAssert)
		{
			var text = getSourcePart?.Invoke()?.Id;
			Tracer.AssertionFailed(cond, () => text, stackFrameDepth + 1);
			return;
		}

		var stackFrame = new StackTrace(true).GetFrame(stackFrameDepth + 1);
		throw new ExpectationFailedException(cond, getSourcePart?.Invoke());
	}
}