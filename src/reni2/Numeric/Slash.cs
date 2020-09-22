using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;

namespace Reni.Numeric
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Slash : TransformationOperation
    {
        public const string TokenId = "/";
        public override string Id => TokenId;

        protected override int Signature(int objSize, int argSize)
            => BitsConst.DivideSize(objSize, argSize);

        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable(base.MakeGeneric);
    }
}