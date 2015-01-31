using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Feature;

namespace Reni.TokenClasses
{
    sealed class TextItem : Definable
    {
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
        public static string Id = "text_item";
    }
}