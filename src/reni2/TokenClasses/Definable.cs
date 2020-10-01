using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass, IDeclarerTokenClass, IValueProvider, IDeclarationItem
    {
        [DisableDump]
        protected string DataFunctionName => Id.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IDeclarationProvider> MakeGeneric
            => this.GenericListFromDefinable();

        internal Result<Parser.Value> CreateForVisit
            (Syntax parent, Parser.Value left, Parser.Value right)
            => ExpressionSyntax.Create(parent, left, this, right);

        Result<Declarer> IDeclarerTokenClass.Get(Syntax syntax)
        {
            if(syntax.Right == null)
            {
                if(syntax.Left == null)
                    return new Declarer(null, this, syntax.SourcePart);

                return syntax.Left.Declarer?.Target.WithName(this, syntax.SourcePart);
            }

            Tracer.FlaggedLine(nameof(syntax) + "=" + syntax);
            return null;
        }

        Result<Parser.Value> IValueProvider.Get(Syntax syntax)
            => ExpressionSyntax.Create(this, syntax);

        bool IDeclarationItem.IsDeclarationPart(Syntax syntax)
            => syntax.IsDeclarationPart;
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