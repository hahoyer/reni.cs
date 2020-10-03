using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass, IDeclarerTokenClass, IValueProvider
    {
        [DisableDump]
        protected string DataFunctionName => Id.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable();

        internal Result<Parser.Syntax> CreateForVisit
            (BinaryTree parent, Parser.Syntax left, Parser.Syntax right)
            => ExpressionSyntax.Create(parent, left, this, right);

        Result<Declarer> IDeclarerTokenClass.Get(BinaryTree binaryTree)
        {
            if(binaryTree.Right == null)
            {
                if(binaryTree.Left == null)
                    return new Declarer(null, this, binaryTree);

                return binaryTree.Left.Declarer?.Target.WithName(this, binaryTree);
            }

            Tracer.FlaggedLine(nameof(binaryTree) + "=" + binaryTree);
            return null;
        }

        Result<Parser.Syntax> IValueProvider.Get(BinaryTree binaryTree, ISyntaxScope scope)
            => ExpressionSyntax.Create(this, binaryTree, scope);

    }

    [BelongsTo(typeof(MainTokenFactory))]
    [Variant(false)]
    [Variant(true)]
    sealed class ConcatArrays : Definable
    {
        public const string TokenId = "<<";
        public const string MutableId = "<<:=";
        internal bool IsMutable {get;}

        public ConcatArrays(bool isMutable) => IsMutable = isMutable;

        [DisableDump]
        internal override IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable(base.MakeGeneric);

        public override string Id => IsMutable ? MutableId : TokenId;
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