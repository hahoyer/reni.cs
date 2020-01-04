using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass, IDeclaratorTokenClass, IValueProvider, IDeclarationItem
    {
        [DisableDump]
        protected string DataFunctionName => Id.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable();

        internal Result<Parser.Value> CreateForVisit
            (Syntax parent, Parser.Value left, Parser.Value right)
            => ExpressionSyntax.Create(parent, left, this, right);

        Result<Declarator> IDeclaratorTokenClass.Get(Syntax syntax)
        {
            if(syntax.Right == null)
            {
                if(syntax.Left == null)
                    return new Declarator(null, this,syntax.SourcePart);

                return syntax.Left.Declarer?.Target.WithName(this,syntax.SourcePart);
            }

            Tracer.FlaggedLine(nameof(syntax) + "=" + syntax);
            return null;
        }

        Result<Parser.Value> IValueProvider.Get(Syntax syntax)
            => ExpressionSyntax.Create(this, syntax);

        bool IDeclarationItem.IsDeclarationPart(Syntax syntax)
        {
            var token = syntax.Token.SourcePart();

            var parentTokenClass = syntax.Parent.TokenClass;
            if(parentTokenClass is Colon)
                return syntax.Parent.Left == syntax;

            if(parentTokenClass is LeftParenthesis ||
                parentTokenClass is Definable ||
                parentTokenClass is ThenToken ||
                parentTokenClass is List ||
                parentTokenClass is Function ||
                parentTokenClass is TypeOperator ||
                parentTokenClass is ElseToken ||
                parentTokenClass is ScannerSyntaxError)
                return false;

            Tracer.FlaggedLine(nameof(syntax) + "=" + syntax);
            return false;
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