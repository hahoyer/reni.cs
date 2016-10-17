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
        internal SyntaxToken(Syntax syntax) { Syntax = syntax; }

        public override Syntax Syntax { get; }

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
            => Syntax.Issues?.Any(item => item.Position == SourcePart)??false;
        [EnableDumpExcept(false)]
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;
        [EnableDumpExcept(false)]
        public override bool IsBrace
            => TokenClass is LeftParenthesis || TokenClass is RightParenthesis;

        [EnableDumpExcept(false)]
        public override bool IsComment
            => Syntax.Issues?.Any(item => item.IssueId == IssueId.EOFInComment)??false;

        [EnableDumpExcept(false)]
        public override bool IsLineComment
            => Syntax.Issues?.Any(item => item.IssueId == IssueId.EOFInLineComment)??false;

        [DisableDump]
        public override string State => Syntax.Token.Characters.Id ?? "";

        public override string Reformat(SourcePart targetPart)
            => new Formatting.Configuration().Create().Reformat(Syntax, targetPart);

        public override IEnumerable<SourcePart> FindAllBelongings(CompilerBrowser compiler)
            => compiler.FindAllBelongings(Syntax)?.Select(item => item.Token.Characters);

        public override Token LocatePosition(int current) => LocatePosition(Syntax, current);
    }
}