using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Type;

namespace Reni.Code.ReplaceVisitor
{
    internal sealed class ReplaceAbsoluteArg : ReplaceArg
    {
        public ReplaceAbsoluteArg(Result actualArg)
            : base(actualArg) { }

        protected override CodeBase Actual { get { return ActualArg.Code; } }
    }
}