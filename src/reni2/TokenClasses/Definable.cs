﻿using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass
    {
        protected override sealed Syntax Terminal(SourcePart token)
            => new DefinableSyntax(token, this);

        protected override sealed Syntax Prefix(SourcePart token, Syntax right)
            => new ExpressionSyntax(null, this, right.ToCompiledSyntax, token);

        protected override sealed Syntax Suffix(Syntax left, SourcePart token)
            => left.SuffixedBy(this, token);

        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => new ExpressionSyntax(left.ToCompiledSyntax, this, right.ToCompiledSyntax, token);

        [DisableDump]
        protected string DataFunctionName => Id.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable();
    }

    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false)]
    [Variant(true)]
    sealed class ConcatArrays : Definable
    {
        public const string TokenId = "<<";
        public const string MutableId = "<<:=";
        internal bool IsMutable { get; }

        public ConcatArrays(bool isMutable) { IsMutable = isMutable; }

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);

        public override string Id => IsMutable ? MutableId : TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Item : Definable
    {
        public const string TokenId = "item";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Count : Definable
    {
        public const string TokenId = "count";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Reference : Definable
    {
        public const string TokenId = "reference";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ArrayReference : Definable
    {
        public const string TokenId = "array_reference";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public override string Id => TokenId;
    }
}