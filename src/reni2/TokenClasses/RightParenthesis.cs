using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
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
        IValueProvider
    {
        internal sealed class Matched : DumpableObject, IType<Syntax>, ITokenClass, IValueProvider
        {
            static string Id => "()";

            Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
            {
                Tracer.Assert(left != null);
                Tracer.Assert(right != null);
                var leftValue = left.Value;
                var rightValue = right.Value;
                return ExpressionSyntax.Create(leftValue.Target, null, rightValue.Target, token)
                    .With(rightValue.Issues.plus(leftValue.Issues));
            }

            Syntax IType<Syntax>.Create(Syntax left, IToken token, Syntax right)
                => right == null ? left : Syntax.CreateSourceSyntax(left, this, token, right);

            string IType<Syntax>.PrioTableId => Id;
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

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        IType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();

        Result<Value> IValueProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            var result = left.GetBracketKernel(Level, token, right);
            var target = result.Item1?.Option.Value ?? new EmptyList(token);

            if(result.Item2 != null)
                return target.With(result.Item2);

            return result.Item1 == null ? new EmptyList(token) : result.Item1.Value;
        }

        Result<Declarator> IDeclaratorTagProvider.Get(Syntax left, SourcePart token, Syntax right)
        {
            var result = left.GetBracketKernel(Level, token, right);
            var target = result.Item1?.Option.Declarator ?? new Declarator(null, null);

            if(result.Item2 != null)
                return target.With(result.Item2);

            if(result.Item1 == null)
                return target.With(IssueId.MissingDeclarationTag.CreateIssue(token));

            NotImplementedMethod(left, token, right, nameof(result), result);
            return null;
        }
    }
}