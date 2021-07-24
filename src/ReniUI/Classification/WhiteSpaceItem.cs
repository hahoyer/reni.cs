using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Classification
{
    sealed class WhiteSpaceItem : Item
    {
        readonly IItem Item;

        internal WhiteSpaceItem(IItem item, BinaryTree anchor)
            : base(anchor)
            => Item = item;

        public override SourcePart SourcePart => Item.SourcePart;
        public override bool IsComment => Lexer.IsMultiLineComment(Item);
        public override bool IsLineComment => Lexer.IsLineComment(Item);
        public override bool IsWhiteSpace => Lexer.IsSpace(Item);
        public override bool IsLineEnd => Lexer.IsLineEnd(Item);
        public override string State => Lexer.Instance.WhiteSpaceId(Item) ?? "";

        [DisableDump]
        public override IEnumerable<SourcePart> ParserLevelGroup
        {
            get { yield break; }
        }
    }
}