using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace ReniUI.Classification
{
    sealed class WhiteSpaceItem : Item
    {
        readonly WhitespaceGroup Item;

        internal WhiteSpaceItem(WhitespaceGroup item, BinaryTree anchor)
            : base(anchor)
            => Item = item;

        public override SourcePart SourcePart => Item.SourcePart;

        public override bool IsComment => Item.Type is WhitespaceGroup.IComment;
        public override bool IsSpace => Item.Type is WhitespaceGroup.ISpace;
        public override bool IsLineEnd => Item.Type is WhitespaceGroup.ILineBreak;
        //public override string State => Lexer.Instance.WhiteSpaceId(Item) ?? "";

        [DisableDump]
        public override IEnumerable<SourcePart> ParserLevelGroup
        {
            get { yield break; }
        }
    }
}