using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Feature;

namespace Reni.Type
{
    sealed class EnableCut
        : TagChild<TypeBase>
            , IForcedConversionProvider<NumberType>
    {
        internal EnableCut(TypeBase parent)
            : base(parent)
        {
            Tracer.Assert(Parent.IsCuttingPossible, Parent.Dump);
        }

        [DisableDump]
        protected override string TagTitle => "enable_cut";

        IEnumerable<IValue> IForcedConversionProvider<NumberType>.Result(NumberType destination)
            => Parent.CutEnabledConversion(destination);
    }
}