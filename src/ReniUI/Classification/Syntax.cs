using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni;
using Reni.TokenClasses;
using Reni.Validation;

namespace ReniUI.Classification
{
    sealed class Syntax : Item
    {
        internal Syntax(BinaryTree anchor)
            : base(anchor) { }

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
            => Issue != null;

        [EnableDumpExcept(false)]
        public override bool IsBraceLike => TokenClass is IBelongingsMatcher;

        [EnableDumpExcept(false)]
        public override bool IsBrace
            => TokenClass is IBracket;

        [EnableDumpExcept(false)]
        public override bool IsPunctuation
            => TokenClass is List;

        [DisableDump]
        public override string State => Token.Characters.Id ?? "";

        public override IEnumerable<SourcePart> ParserLevelGroup
            => Master
                .Anchor
                .Items
                .Where(node => Anchor.TokenClass.IsBelongingTo(node.TokenClass))
                .Select(node => node.Token.Characters);

        [DisableDump]
        public override Issue Issue => Master.MainAnchor.Issue ?? Master.Issue;
    }
}