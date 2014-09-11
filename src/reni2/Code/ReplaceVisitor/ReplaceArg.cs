using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    /// <summary>
    ///     Handle argument replaces
    /// </summary>
    abstract class ReplaceArg : Base
    {
        static int _nextObjectId;
        readonly Result _actualArg;

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
            if(ActualArg.Type == visitedObject.Type)
                return Actual;
            var conversion = ActualArg
                .Type
                .ObviousExactConversion(Category.Code.Typed, visitedObject.Type);
            if(conversion != null)
                return conversion
                    .ReplaceArg(ActualArg)
                    .Code;
            throw new TypeException(Actual, visitedObject);
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