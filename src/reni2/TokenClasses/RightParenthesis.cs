using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    [BelongsTo(typeof(DeclarationTokenFactory))]
    [Variant(0)]
    [Variant(1)]
    [Variant(2)]
    [Variant(3)]
    sealed class RightParenthesis : TokenClass,
        IBelongingsMatcher,
        IBracketMatch<Syntax>,
        IDeclaratorTagProvider,
        IValueProvider,
        IDefaultScopeProvider
    {
        internal sealed class Matched : DumpableObject, IParserTokenType<Syntax>, ITokenClass, IValueProvider
        {
            static string Id => "()";

            Result<Value> IValueProvider.Get(Syntax syntax)
            {
                Tracer.Assert(syntax.Left != null);
                Tracer.Assert(syntax.Right != null);
                var leftValue = syntax.Left.Value;
                var rightValue = syntax.Right.Value;
                return ExpressionSyntax.Create(syntax, leftValue.Target, null, rightValue.Target)
                    .With(rightValue.Issues.plus(leftValue.Issues));
            }

            Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
                => right == null ? left : Syntax.CreateSourceSyntax(left, this, token, right);

            string IParserTokenType<Syntax>.PrioTableId => Id;
            string ITokenClass.Id => Id;
        }

        public static string TokenId(int level)
            => level == 0 ? PrioTable.EndOfText : "}])".Substring(level - 1, 1);

        public RightParenthesis(int level) { Level = level; }

        [DisableDump]
        internal int Level { get; }
        [DisableDump]
        public override string Id => TokenId(Level);
        [DisableDump]
        internal override bool IsVisible => Level != 0;
        [DisableDump]
        internal bool IsFrameToken => Level == 0;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        IParserTokenType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();


        Result<Value> IValueProvider.Get(Syntax syntax)
        {
            var result = syntax.Left.GetBracketKernel(Level, syntax);
            var target = result.Item1?.Option.Value ?? new EmptyList(syntax);

            if(result.Item2 != null)
                return target.With(result.Item2);

            return result.Item1 == null ? new EmptyList(syntax) : result.Item1.Value;
        }

        Result<Declarator> IDeclaratorTagProvider.Get(Syntax syntax)
        {
            var result = syntax.Left.GetBracketKernel(Level, syntax);
            var target = result.Item1?.Option.Declarator ?? new Declarator(null, null);

            if(result.Item2 != null)
                return target.With(result.Item2);

            if(result.Item1 == null)
                return target.With(IssueId.MissingDeclarationTag.Create(syntax));

            NotImplementedMethod(syntax, nameof(result), result);
            return null;
        }

        bool IDefaultScopeProvider.MeansPublic => Level == 3;
    }
}