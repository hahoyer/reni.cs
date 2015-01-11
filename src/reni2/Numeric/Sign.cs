using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Numeric
{
    sealed class Minus
        : TransformationOperation
    {
        public const string Id = "-";
        public Minus() { Name = Id; }
        protected override int Signature(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }

    sealed class Plus
        : TransformationOperation
    {
        protected override int Signature(int objSize, int argSize) { return BitsConst.PlusSize(objSize, argSize); }
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }

    sealed class Negate : Definable
    {
        public const string Id = "negate";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }

    sealed class AlignToken : Definable
    {
        public const string Id = "!!!";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }

}