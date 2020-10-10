using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;
using ReniUI.Helper;

namespace ReniUI.Classification
{
    sealed class SyntaxToken : Token
    {
        internal override Syntax Syntax { get; }
        internal SyntaxToken(Syntax syntax) => Syntax = syntax;

        TokenClass TokenClass => Syntax.TokenClass as TokenClass;

        public override SourcePart SourcePart => Syntax.Token.Characters;

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
            => Syntax.Issues?.Any(item => item.Position == SourcePart) ?? false;

        [EnableDumpExcept(false)]
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;

        [EnableDumpExcept(false)]
        public override bool IsBrace
            => TokenClass is LeftParenthesis || TokenClass is RightParenthesis;

        [EnableDumpExcept(false)]
        public override bool IsComment
            => Syntax.Issues.Any(item => item.IssueId == IssueId.EOFInComment);

        [EnableDumpExcept(false)]
        public override bool IsLineComment
            => Syntax.Issues.Any(item => item.IssueId == IssueId.EOFInLineComment);

        [DisableDump]
        public override string State => Syntax.Token.Characters.Id ?? "";

        [Obsolete("", true)]
        public override IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler)
            => compiler.FindAllBelongings(Syntax.BinaryTreeSyntax)?.Select(item => item.Token.Characters);
    }
}