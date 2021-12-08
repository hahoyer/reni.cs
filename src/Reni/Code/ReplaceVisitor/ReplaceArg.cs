using System;
using hw.Helper;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    ///     Handle argument replaces
    /// </summary>
    abstract class ReplaceArg : Base
    {
        static int NextObjectId;

        internal ReplaceArg(ResultCache actualArg)
            : base(NextObjectId++)
        {
            (actualArg != null).Assert(() => "actualArg != null");
            ActualArg = actualArg;
        }

        [DisableDump]
        protected ResultCache ActualArg { get; }

        protected abstract CodeBase ActualCode { get; }

        internal override CodeBase Arg(Arg visitedObject)
        {
            if(ActualArg.Type == visitedObject.Type)
                return ActualCode;
            throw new TypeException(ActualCode, visitedObject);
        }

        [Dump("Dump")]
        internal sealed class TypeException : Exception
        {
            readonly CodeBase Actual;
            readonly Arg VisitedObject;

            public TypeException(CodeBase actual, Arg visitedObject)
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
    }
}