using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;
using Reni.Type;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    ///     Handle argument replaces
    /// </summary>
    internal abstract class ReplaceArg : Base
    {
        private static int _nextObjectId;
        private readonly Result _actualArg;

        internal ReplaceArg(Result actualArg)
            : base(_nextObjectId++)
        {
            Tracer.Assert(actualArg != null, () => "actualArg != null");
            Tracer.Assert(actualArg.HasCode, () => "actualArg.HasCode");
            //Tracer.Assert(actualArg.HasType, () => "actualArg.HasType");
            _actualArg = actualArg;
        }

        [DisableDump]
        protected Result ActualArg { get { return _actualArg; } }

        protected abstract CodeBase Actual { get; }

        internal override CodeBase Arg(Arg visitedObject)
        {
            if(ActualArg.Type != visitedObject.Type)
                throw new SizeException(Actual, visitedObject);
            return Actual;
        }

        [Dump("Dump")]
        internal sealed class SizeException : Exception
        {
            private readonly CodeBase _actual;
            private readonly Arg _visitedObject;

            public SizeException(CodeBase actual, Arg visitedObject)
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

                return "SizeException\n{"
                       + data.Indent()
                       + "\n}";
            }
        }
    }
}