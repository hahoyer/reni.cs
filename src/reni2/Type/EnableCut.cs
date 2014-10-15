using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Feature;

namespace Reni.Type
{
    sealed class EnableCut : TagChild<TypeBase>
    {
        internal EnableCut(TypeBase parent)
            : base(parent)
        {
            Tracer.Assert(Parent.IsCuttingPossible, Parent.Dump);
        }

        [DisableDump]
        protected override string TagTitle { get { return "enable_cut"; } }

        internal override IEnumerable<SearchResult> ConvertersForType(TypeBase destination, IConversionParameter parameter)
        {
            return Parent.ConvertersForType(destination, parameter.EnsureEnableCut);
        }
    }
}