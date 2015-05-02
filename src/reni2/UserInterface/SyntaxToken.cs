using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Formatting;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.UserInterface
{
    sealed class SyntaxToken : TokenInformation
    {
        internal SyntaxToken(SourceSyntax sourceSyntax) { SourceSyntax = sourceSyntax; }

        internal SourceSyntax SourceSyntax { get; }

        TokenClass TokenClass => SourceSyntax.TokenClass as TokenClass;

        public override SourcePart SourcePart => SourceSyntax.Token.Characters;

        [EnableDumpExcept(false)]
        public override bool IsKeyword => !IsIdentifier && !IsNumber && !IsText;
        [EnableDumpExcept(false)]
        public override bool IsIdentifier => TokenClass is Definable;
        [EnableDumpExcept(false)]
        public override bool IsText => TokenClass is Text;
        [EnableDumpExcept(false)]
        public override bool IsNumber => TokenClass is Number;
        [EnableDumpExcept(false)]
        public override bool IsError => SourceSyntax.Issues.Any();
        [EnableDumpExcept(false)]
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;

        [EnableDumpExcept(false)]
        public override bool IsComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInComment);

        [EnableDumpExcept(false)]
        public override bool IsLineComment
            => SourceSyntax.Issues.Any(item => item.IssueId == IssueId.EOFInLineComment);

        [DisableDump]
        public override string State => SourceSyntax.Token.Id ?? "";

        public override string Reformat(SourcePart targetPart)
            => SmartFormat.Reformat(SourceSyntax, targetPart);

        public override IEnumerable<SourcePart> FindAllBelongings(Compiler compiler)
            => compiler.FindAllBelongings(SourceSyntax)?.Select(item => item.Token.Characters);
    }
}