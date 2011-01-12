using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using JetBrains.Annotations;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    /// Handle argument replaces
    /// </summary>
    internal abstract class ReplaceArg : Base
    {
        private static int _nextObjectId;
        private readonly CodeBase _actualArg;

        internal ReplaceArg(CodeBase actualArg)
            : base(_nextObjectId++)
        {
            Tracer.Assert(actualArg != null, () => "actualArg != null");
            _actualArg = actualArg;
        }

        [IsDumpEnabled(false)]
        protected CodeBase ActualArg { get { return _actualArg; } }

        protected abstract CodeBase Actual { get; }
        
        internal override CodeBase Arg(Arg visitedObject)
        {
            if(Actual.Size != visitedObject.Size)
                throw new SizeException(Actual, visitedObject);
            return Actual;
        }

        [Dump("Dump")]
        internal class SizeException : Exception
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
                var data = "\nActual=" 
                    + Tracer.Dump(_actual) 
                    + "\nVisitedObject=" 
                    + Tracer.Dump(_visitedObject);

                return "SizeException\n{"
                       + data.Indent()
                       + "\n}";
            }
        }
    }
}
