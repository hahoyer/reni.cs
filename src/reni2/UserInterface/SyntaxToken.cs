using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.UserInterface
{
    sealed class SyntaxToken : TokenInformation
    {
        internal SyntaxToken(SourceSyntax sourceSyntax) { SourceSyntax = sourceSyntax; }

        internal SourceSyntax SourceSyntax { get; }

        TokenClass TokenClass => SourceSyntax.TokenClass as TokenClass;

        public override SourcePart TokenSourcePart => SourceSyntax.Token.Characters;
        public override SourcePart SourcePart => SourceSyntax.SourcePart;

        public override bool IsKeyword => !IsIdentifier && !IsNumber && !IsText;
        public override bool IsIdentifier => TokenClass is Definable;
        public override bool IsText => TokenClass is Text;
        public override bool IsNumber => TokenClass is Number;
        public override bool IsError => SourceSyntax.Issues.Any();
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;

        public override bool IsComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInComment);

        public override bool IsLineComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInLineComment);

        public override string State => SourceSyntax.Token.Id ?? "";

        public override IEnumerable<SourcePart> FindAllBelongings(Compiler compiler)
            => compiler.FindAllBelongings(SourceSyntax)?.Select(item => item.Token.Characters);

        public override string Reformat => SourceSyntax.Reformat();
    }
}