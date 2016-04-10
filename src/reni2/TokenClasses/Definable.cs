using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Feature;
using Reni.Parser;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass, IDeclaratorTokenClass
    {
        protected override Result<Parser.Value> Suffix(Syntax left, SourcePart token)
        {
            var d = left.Declarator;
            if(d != null)
                return null;

            NotImplementedMethod(left, token);
            return null;
        }

        protected sealed override Result<Parser.Value> Infix
            (Parser.Value left, SourcePart token, Parser.Value right)
            => ExpressionSyntax.Create(left, this, right, token);

        [DisableDump]
        protected string DataFunctionName => Id.Symbolize();

        [DisableDump]
        internal virtual IEnumerable<IDeclarationProvider> Genericize
            => this.GenericListFromDefinable();

        public Result<Parser.Value> CreateForVisit(Parser.Value left, Parser.Value right)
        {
            NotImplementedMethod(left, right);
            return null;
        }

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