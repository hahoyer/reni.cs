using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    [BelongsTo(typeof(MainTokenFactory))]
    sealed class ElseToken : TokenClass
    {
        public const string TokenId = "else";
        public override string Id => TokenId;

        protected override Syntax Suffix(Syntax left, SourcePart token)
            => IssueId.MissingElseBody.Syntax(token);

        protected override Syntax Infix(Syntax left, SourcePart token, Syntax right)
            => left.CreateElseSyntax(right.ToCompiledSyntax);

        protected override Syntax Terminal(SourcePart token)
            => IssueId.MissingThen.Syntax(token);

        protected override Syntax Prefix(SourcePart token, Syntax right)
            => IssueId.MissingThen.Syntax(token);
    }
}