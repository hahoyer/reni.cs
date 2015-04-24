using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Numeric
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Star : TransformationOperation
    {
        public const string TokenId = "*";
        public override string Id => TokenId;
        protected override int Signature(int objSize, int argSize) => BitsConst.MultiplySize(objSize, argSize);
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}