using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;

namespace Reni.Type
{
    [Obsolete("",true)]
    internal sealed class AutomaticRef : Ref
    {
        internal AutomaticRef(TypeBase target, RefAlignParam refAlignParam)
            : base(target, refAlignParam) { }

        protected override string ShortName { get { return "automatic_ref"; } }
    }
}