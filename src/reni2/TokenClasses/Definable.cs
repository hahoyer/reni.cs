using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
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
            (Parser.Value left, Parser.Value right, SourcePart token)
            => ExpressionSyntax.Create(left, this, right, token);

        Result<Declarator> IDeclaratorTokenClass.Get(Syntax left, SourcePart token, Syntax right)
        {
            if(right == null)
            {
                if(left == null)
                    return new Declarator(null, this);

                var d = left.Declarator;
                if(d != null)
                    return d.Target.WithName(this);
            }

            NotImplementedMethod(left, token, right);
            return null;
        }

        Result<Parser.Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
            => ExpressionSyntax.Create(left, this, right, token);

        bool IDeclarationItem.IsDeclarationPart(Syntax syntax)
        {
            var t = syntax.Token.SourcePart;

            var p = syntax.Parent.TokenClass;
            if(p is Colon)
                return syntax.Parent.Left == syntax;

            if(p is LeftParenthesis || p is Definable || p is ThenToken
                || p is List || p is Function || p is TypeOperator
                || p is ElseToken)
                return false;

            NotImplementedMethod(syntax);
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