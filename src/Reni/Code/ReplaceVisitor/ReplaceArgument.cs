using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;

namespace Reni.Code.ReplaceVisitor;

/// <summary>
///     Handle argument replaces
/// </summary>
abstract class ReplaceArgument : Base
{
    [Dump("Dump")]
    internal sealed class TypeException : Exception
    {
        readonly CodeBase Actual;
        readonly Argument VisitedObject;

        public TypeException(CodeBase actual, Argument visitedObject)
        {
            Actual = actual;
            VisitedObject = visitedObject;
        }

        [UsedImplicitly]
        public string Dump()
        {
            var data = "\nVisitedObject="
                + Tracer.Dump(VisitedObject)
                + "\nActual="
                + Tracer.Dump(Actual);

            return "TypeException\n{"
                + data.Indent()
                + "\n}";
        }
    }

    static int NextObjectId;

    [DisableDump]
    protected ResultCache ActualArg { get; }

    internal ReplaceArgument(ResultCache actualArg)
        : base(NextObjectId++)
    {
        (actualArg != null).Assert(() => "actualArg != null");
        ActualArg = actualArg;
    }

    protected abstract CodeBase ActualCode { get; }

    internal override CodeBase Arg(Argument visitedObject)
    {
        if(ActualArg.Type == visitedObject.Type)
            return ActualCode;
        throw new TypeException(ActualCode, visitedObject);
    }
}