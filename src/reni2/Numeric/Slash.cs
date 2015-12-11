using System.Collections.Generic;
using System.Linq;
using System;
using hw.DebugFormatter;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Numeric
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Slash : TransformationOperation
    {
        public const string TokenId = "/";
        public override string Id => TokenId;
        protected override int Signature(int objSize, int argSize) => BitsConst.DivideSize(objSize, argSize);
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}