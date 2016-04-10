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
    sealed class LeftParenthesis : TokenClass, IBelongingsMatcher
    {
        public static string TokenId(int level)
            => level == 0 ? PrioTable.BeginOfText : "{[(".Substring(level-1, 1);

        public LeftParenthesis(int level) { Level = level; }
        [DisableDump]
        internal int Level { get; }
        [DisableDump]
        public override string Id => TokenId(Level);
        [DisableDump]
        internal override bool IsVisible => Level != 0;

        protected override Checked<Value> Infix(Value left, SourcePart token, Value right) => null;
        protected override Checked<Value> Prefix(SourcePart token, Value right) => null;
        protected override Checked<Value> Prefix(SourcePart token, TokenClasses.Syntax right) => null;
        protected override Checked<Value> Suffix(Value left, SourcePart token) => null;
        protected override Checked<Value> Terminal(SourcePart token) => null;

        sealed class Syntax : OldSyntax
        {
            internal override SourcePart Token { get; }
            readonly int Level;

            [EnableDump]
            OldSyntax Right { get; }
            [EnableDump]
            OldSyntax Left { get; }

            public Syntax(OldSyntax left, int level, SourcePart token, OldSyntax right)
            {
                Left = left;
                Token = token;
                Level = level;
                Right = right;
            }

            [DisableDump]
            protected override IEnumerable<OldSyntax> DirectChildren
            {
                get
                {
                    yield return Left;
                    yield return Right;
                }
            }

            [DisableDump]
            internal override Checked<Value> ToCompiledSyntax
            {
                get
                {
                    if(Left == null)
                        return (Right ?? new EmptyList(Token)).ToCompiledSyntax;

                    if(Right != null)
                        NotImplementedFunction();

                    return Left.ToCompiledSyntax;
                }
            }

            internal override Checked<OldSyntax> Match(int level, SourcePart token)
            {
                if(Level != level)
                    return new Checked<OldSyntax>
                        (this, IssueId.ExtraLeftBracket.CreateIssue(Token));

                var innerPart = Right ?? new EmptyList(token);

                if(Level == 0)
                {
                    Tracer.Assert(Left == null);
                    return Checked<OldSyntax>.From(innerPart.ToCompiledSyntax);
                }

                if(Left == null)
                    return innerPart;

                NotImplementedMethod(level, token);
                return null;
            }
        }

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as RightParenthesis)?.Level == Level;
    }
}