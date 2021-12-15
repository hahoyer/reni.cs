using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.TokenClasses.Whitespace;

namespace ReniUI.Classification
{
    sealed class WhiteSpaceItem : Item
    {
        readonly IWhitespaceItem Item;

        internal WhiteSpaceItem(Reni.TokenClasses.WhiteSpaceItem item, BinaryTree anchor)
            : base(anchor)
            => Item = item;

        public override SourcePart SourcePart => Item.SourcePart;

        public override bool IsComment => GetItem<IComment>() != null;
        public override bool IsSpace => GetItem<ISpace>() != null;
        public override bool IsLineEnd => GetItem<ILineBreak>() != null;

        [DisableDump]
        public override IEnumerable<SourcePart> ParserLevelGroup
        {
            get { yield break; }
        }

        internal override IWhitespaceItem GetItem<TItemType>() 
            => Item.GetItem<TItemType>();
    }

}