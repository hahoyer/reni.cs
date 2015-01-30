using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Code.ReplaceVisitor
{
    sealed class ReplaceAbsoluteArg : ReplaceArg
    {
        public ReplaceAbsoluteArg(ResultCache actualArg)
            : base(actualArg) { }

        protected override CodeBase ActualCode => ActualArg.Code;
    }
}