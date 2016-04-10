﻿using System;
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
        IDeclaratorTagProvider
    {
        internal sealed class Matched : DumpableObject, IType<Syntax>, ITokenClass
        {
            static string Id => "()";

            Checked<Value> ITokenClass.GetValue(Syntax left, SourcePart token, Syntax right)
            {
                Tracer.Assert(left != null);
                Tracer.Assert(right != null);
                var leftValue = left.Value;
                var rightValue = right.Value;
                return ExpressionSyntax.Create(leftValue.Value, null, rightValue.Value, token)
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

        protected override Checked<Value> Suffix(Syntax left, SourcePart token)
        {
            var syntax = left.GetBracketKernel();
            if(syntax == null)
                return new EmptyList(token);

            var value = syntax.Value;
            if(value != null)
                return value;

            NotImplementedMethod(left, token);
            return null;
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        IType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();

        Checked<DeclaratorTags> IDeclaratorTagProvider.Get
            (Syntax left, SourcePart token, Syntax right)
        {
            var syntax = left.GetBracketKernel(right);
            if(syntax == null)
                return new Checked<DeclaratorTags>
                    (
                    DeclaratorTags.Create(token),
                    IssueId.MissingDeclarationTag.CreateIssue(token));

            NotImplementedMethod(left, token, right, nameof(syntax), syntax);
            return null;
        }
    }
}