using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.Formatting;

namespace ReniUI.Classification
{
    sealed class SyntaxToken : Token
    {
        internal SyntaxToken(SourceSyntax sourceSyntax) { SourceSyntax = sourceSyntax; }

        public override SourceSyntax SourceSyntax { get; }

        TokenClass TokenClass => SourceSyntax.TokenClass as TokenClass;

        public override SourcePart SourcePart => SourceSyntax.Token.Characters;

        [EnableDumpExcept(false)]
        public override bool IsKeyword => !IsIdentifier && !IsNumber && !IsText && !IsBrace;
        [EnableDumpExcept(false)]
        public override bool IsIdentifier => TokenClass is Definable;
        [EnableDumpExcept(false)]
        public override bool IsText => TokenClass is Text;
        [EnableDumpExcept(false)]
        public override bool IsNumber => TokenClass is Number;
        [EnableDumpExcept(false)]
        public override bool IsError 
            => SourceSyntax.Issues?.Any(item => item.Position == SourcePart)??false;
        [EnableDumpExcept(false)]
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;
        [EnableDumpExcept(false)]
        public override bool IsBrace
            => TokenClass is LeftParenthesis || TokenClass is RightParenthesis;

        [EnableDumpExcept(false)]
        public override bool IsComment
            => SourceSyntax.Issues?.Any(item => item.IssueId == IssueId.EOFInComment)??false;

        [EnableDumpExcept(false)]
        public override bool IsLineComment
            => SourceSyntax.Issues?.Any(item => item.IssueId == IssueId.EOFInLineComment)??false;

        [DisableDump]
        public override string State => SourceSyntax.Token.Id ?? "";

        public override string Reformat(SourcePart targetPart)
            => new Formatting.Configuration().Create().Reformat(SourceSyntax, targetPart);

        public override IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler)
            => compiler.FindAllBelongings(SourceSyntax)?.Select(item => item.Token.Characters);

        public override Token LocatePosition(int current) => LocatePosition(SourceSyntax, current);
    }
}