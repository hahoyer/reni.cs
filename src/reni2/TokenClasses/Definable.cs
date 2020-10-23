using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Parser;
using Reni.SyntaxFactory;
using Reni.SyntaxTree;

namespace Reni.TokenClasses
{
    abstract class Definable
        : TokenClass, IDeclarationTagToken, IValueToken
    {
        [DisableDump]
        protected string DataFunctionName => Id.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable();

        IValueProvider IValueToken.Provider => Factory.Definable;

        internal ValueSyntax CreateForVisit
            (BinaryTree parent, ValueSyntax left, ValueSyntax right, FrameItemContainer frameItems)
            => ExpressionSyntax.Create(parent, left, this, right, frameItems);
    }

    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false)]
    [Variant(true)]
    sealed class ConcatArrays : Definable
    {
        public const string TokenId = "<<";
        public const string MutableId = "<<:=";
        internal bool IsMutable { get; }

        public ConcatArrays(bool isMutable) => IsMutable = isMutable;

        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable(base.MakeGeneric);

        public override string Id => IsMutable? MutableId : TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class Count : Definable
    {
        public const string TokenId = "count";

        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable(base.MakeGeneric);

        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class StableReference : Definable
    {
        public const string TokenId = "stable_reference";

        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable(base.MakeGeneric);

        public override string Id => TokenId;
    }

    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ArrayReference : Definable
    {
        public const string TokenId = "array_reference";

        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable(base.MakeGeneric);

        public override string Id => TokenId;
    }
}