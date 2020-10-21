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
        internal override Syntax Master { get; }
        internal SyntaxToken(Syntax master) => Master = master;

        TokenClass TokenClass => Master.TokenClass as TokenClass;

        public override SourcePart SourcePart => Master.Token.Characters;

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
            => Issues?.Any(item => item.Position == SourcePart) ?? false;

        [EnableDumpExcept(false)]
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;

        [EnableDumpExcept(false)]
        public override bool IsBrace
            => TokenClass is LeftParenthesis || TokenClass is RightParenthesis;

        [EnableDumpExcept(false)]
        public override bool IsComment
            => Issues.Any(item => item.IssueId == IssueId.EOFInComment);

        [EnableDumpExcept(false)]
        public override bool IsLineComment
            => Issues.Any(item => item.IssueId == IssueId.EOFInLineComment);

        [DisableDump]
        IEnumerable<Issue> Issues
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        public override string State => Master.Token.Characters.Id ?? "";

        [DisableDump]
        public override IEnumerable<SourcePart> ParserLevelBelongings
            => Master.ParserLevelBelongings?.Select(item => item.Token.Characters);
    }
}