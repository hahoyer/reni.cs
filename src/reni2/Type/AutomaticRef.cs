using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;

namespace Reni.Type
{
    [Serializable]
    internal sealed class AutomaticRef : Ref
    {
        internal AutomaticRef(TypeBase target, RefAlignParam refAlignParam)
            : base(target, refAlignParam) { }

        protected override string ShortName { get { return "automatic_ref"; } }
        internal override AutomaticRef CreateAutomaticRef() { return this; }
        internal override bool IsConvertableTo_Implementation(TypeBase dest, ConversionFeature conversionFeature) { return IsConvertableTo_Implementation<AutomaticRef>(dest, conversionFeature); }
        protected override Result ConvertTo_Implementation(Category category, TypeBase dest) { return ConvertTo_Implementation<AutomaticRef>(category, dest); }
    }
}