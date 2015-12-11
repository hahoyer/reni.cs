using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.DebugFormatter;
using JetBrains.Annotations;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    ///     Handle argument replaces
    /// </summary>
    abstract class ReplaceArg : Base
    {
        static int _nextObjectId;

        internal ReplaceArg(ResultCache actualArg)
            : base(_nextObjectId++)
        {
            Tracer.Assert(actualArg != null, () => "actualArg != null");
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
            readonly CodeBase _actual;
            readonly Arg _visitedObject;

            public TypeException(CodeBase actual, Arg visitedObject)
            {
                _actual = actual;
                _visitedObject = visitedObject;
            }

            [UsedImplicitly]
            public string Dump()
            {
                var data = "\nVisitedObject="
                    + Tracer.Dump(_visitedObject)
                    + "\nActual="
                    + Tracer.Dump(_actual);

                return "TypeException\n{"
                    + data.Indent()
                    + "\n}";
            }
        }
    }
}