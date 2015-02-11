using System.Collections.Generic;
using System.Linq;
using System;
using hw.Debug;
using Reni.Feature;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ReassignToken : Definable, ITokenClassWithId
    {
        public const string Id = ":=";
        string ITokenClassWithId.Id => Id;
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ForceMutabilityToken : Definable, ITokenClassWithId
    {
        public const string Id = "force_mutability";
        string ITokenClassWithId.Id => Id;
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Mutable : Definable, ITokenClassWithId
    {
        public const string Id = "mutable";
        string ITokenClassWithId.Id => Id;
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class EnableReinterpretation : Definable, ITokenClassWithId
    {
        public const string Id = "enable_reinterpretation";
        string ITokenClassWithId.Id => Id;
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize => this.GenericListFromDefinable(base.Genericize);
    }
}