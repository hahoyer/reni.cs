using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Type
{
    sealed class EnableCut : TagChild<TypeBase>
    {
        internal EnableCut(TypeBase parent)
            : base(parent) { Tracer.Assert(Parent.IsCuttingPossible, Parent.Dump); }

        [DisableDump]
        protected override string TagTitle { get { return "enable_cut"; } }
    }
}