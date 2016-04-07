using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass
    {
        protected override sealed Checked<OldSyntax> OldTerminal(SourcePart token)
            => new DefinableSyntax(token, this);

        protected override sealed Checked<OldSyntax> Prefix(SourcePart token, OldSyntax right)
            => Checked<OldSyntax>
                .From(ExpressionSyntax.Create(null, this, right, token));

        protected override sealed Checked<OldSyntax> Suffix(OldSyntax left, SourcePart token)
            => left.SuffixedBy(this, token);

        protected override sealed Checked<OldSyntax> Infix(OldSyntax left, SourcePart token, OldSyntax right)
            => Checked<OldSyntax>
                .From(ExpressionSyntax.Create(left, this, right, token));

        [DisableDump]
        protected string DataFunctionName => Id.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable();

        public Checked<Parser.Value> CreateForVisit(Parser.Value left, Parser.Value right)
        {
            NotImplementedMethod(left, right);
            return null;

        }
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
        internal override IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable(base.Genericize);

        public override string Id => IsMutable ? MutableId : TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Count : Definable
    {
        public const string TokenId = "count";
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class StableReference : Definable
    {
        public const string TokenId = "stable_reference";
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ArrayReference : Definable
    {
        public const string TokenId = "array_reference";
        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable(base.Genericize);
        public override string Id => TokenId;
    }
}