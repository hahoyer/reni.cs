using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Type
{
    [Serializable]
    internal sealed class EnableCut : TagChild
    {
        internal EnableCut(TypeBase parent)
            : base(parent) { }

        protected override string TagTitle { get { return "enable_cut"; } }

        internal override bool IsConvertableToImplementation(TypeBase dest, ConversionParameter conversionParameter) { return base.IsConvertableToImplementation(dest, conversionParameter.EnableCut); }
    }
}