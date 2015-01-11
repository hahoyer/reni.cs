using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.TokenClasses;

namespace Reni.Numeric
{
    sealed class Minus
        : TransformationOperation
    {
        public const string Id = "-";
        protected override int Signature(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    sealed class Plus
        : TransformationOperation
    {
        protected override int Signature(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    sealed class Negate : Definable
    {
        public const string Id = "negate";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    sealed class AlignToken : Definable
    {
        public const string Id = "!!!";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}