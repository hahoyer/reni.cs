using System;
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
        protected override sealed Syntax Terminal(Token token)
            => Syntax(token);

        protected override sealed Syntax Prefix(Token token, Syntax right)
            => new ExpressionSyntax(null, Syntax(token), right.ToCompiledSyntax);

        protected override sealed Syntax Suffix(Syntax left, Token token)
            => left.SuffixedBy(Syntax(token));

        protected override sealed Syntax Infix(Syntax left, Token token, Syntax right)
            => new ExpressionSyntax(left.ToCompiledSyntax, Syntax(token), right.ToCompiledSyntax);

        [DisableDump]
        protected string DataFunctionName => Name.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable();

        DefinableTokenSyntax Syntax(Token token) => new DefinableTokenSyntax(this, token);
    }

    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false)]
    [Variant(true)]
    sealed class ConcatArrays : Definable, ITokenClassWithId
    {
        public const string Id = "<<";
        public const string MutableId = "<<:=";
        internal bool IsMutable { get; }

        public ConcatArrays(bool isMutable) { IsMutable = isMutable; }

        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);

        string ITokenClassWithId.Id => IsMutable ? MutableId : Id;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Item : Definable, ITokenClassWithId
    {
        public const string Id = "item";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Count : Definable, ITokenClassWithId
    {
        public const string Id = "count";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Reference : Definable, ITokenClassWithId
    {
        public const string Id = "reference";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ArrayReference : Definable, ITokenClassWithId
    {
        public const string Id = "array_reference";
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        string ITokenClassWithId.Id => Id;
    }
}