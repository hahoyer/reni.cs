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
    sealed class RightParenthesis
        : TokenClass
            , IBelongingsMatcher
            , IBracketMatch<Syntax>
    {
        internal sealed class Matched : TokenClass, IExclamationTagProvider
        {
            [DisableDump]
            internal override bool IsVisible => false;
            protected override Checked<Value> Suffix(Syntax left, SourcePart token)
                => left.GetBracketKernel(token).Value;

            [DisableDump]
            public override string Id => "()";

            Checked<ExclamationSyntaxList> IExclamationTagProvider.GetTags(Syntax left, SourcePart token, Syntax right)
            {
                var syntax = left.GetBracketKernel(token);
                if(syntax == null)
                    return new Checked<ExclamationSyntaxList>(
                        ExclamationSyntaxList.Create(token),
                        IssueId.MissingDeclarationTag.CreateIssue(token));

                NotImplementedMethod(left, token, right, nameof(syntax), syntax);
                return null;
            }
        }

        public static string TokenId(int level)
            => level == 0 ? PrioTable.EndOfText : "\0}])".Substring(level, 1);

        public RightParenthesis(int level) { Level = level; }

        [DisableDump]
        internal int Level { get; }
        [DisableDump]
        public override string Id => TokenId(Level);
        [DisableDump]
        internal override bool IsVisible => Level != 0;
        protected override Checked<Value> Suffix(Syntax left, SourcePart token) => null;

        bool IBelongingsMatcher.IsBelongingTo(IBelongingsMatcher otherMatcher)
            => (otherMatcher as LeftParenthesis)?.Level == Level;

        IType<Syntax> IBracketMatch<Syntax>.Value { get; } = new Matched();
    }
}