using System.Collections.Generic;
using System.Linq;
using System;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Basics;
using Reni.Feature;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Numeric
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Minus
        : TransformationOperation
    {
        public const string TokenId = "-";
        protected override int Signature(int objSize, int argSize) => BitsConst.PlusSize(objSize, argSize);
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Plus
        : TransformationOperation
    {
        public const string TokenId = "+";
        protected override int Signature(int objSize, int argSize) => BitsConst.PlusSize(objSize, argSize);
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Negate : Definable
    {
        public const string TokenId = "negate";
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class AlignToken : Definable
    {
        public const string TokenId = "!!!";
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric => this.GenericListFromDefinable(base.MakeGeneric);
        public override string Id => TokenId;
    }
}