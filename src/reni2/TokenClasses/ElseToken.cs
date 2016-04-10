using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        protected override Result<OldSyntax> OldSuffix(OldSyntax left, SourcePart token)
            => IssueId.MissingElseBody.Syntax(token);

        protected override Result<OldSyntax> OldInfix(OldSyntax left, SourcePart token, OldSyntax right)
            => left.CreateElseSyntax(right.ToCompiledSyntax);

        protected override Result<OldSyntax> OldTerminal(SourcePart token)
            => IssueId.MissingThen.Syntax(token);

        protected override Result<OldSyntax> OldPrefix(SourcePart token, OldSyntax right)
            => IssueId.MissingThen.Syntax(token, right);

    }
}