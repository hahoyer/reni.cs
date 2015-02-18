using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.ReniParser;
using Reni.TokenClasses;

namespace Reni.Numeric
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Minus
        : TransformationOperation, ITokenClassWithId
    {
        public const string Id = "-";
        public Minus() { Name = Id; }
        protected override int Signature(int objSize, int argSize) => BitsConst.PlusSize(objSize, argSize);
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Plus
        : TransformationOperation, ITokenClassWithId
    {
        public const string Id = "+";
        protected override int Signature(int objSize, int argSize) => BitsConst.PlusSize(objSize, argSize);
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Negate : Definable, ITokenClassWithId
    {
        public const string Id = "negate";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class AlignToken : Definable, ITokenClassWithId
    {
        public const string Id = "!!!";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }
}