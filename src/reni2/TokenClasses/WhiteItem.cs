using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses {
    class WhiteItem : DumpableObject, IFormatItem
    {
        readonly IItem Item;
        readonly Syntax Syntax;

        public WhiteItem(IItem item, Syntax syntax)
        {
            Item = item;
            Syntax = syntax;
        }

        [EnableDump]
        ITokenClass IFormatItem.TokenClass => null;

        [EnableDump]
        SourcePart IFormatItem.Content => Item.SourcePart;

        [EnableDump]
        bool IFormatItem.IsEssential => Lexer.IsComment(Item) || Lexer.IsLineComment(Item);
        [EnableDump]
        bool IFormatItem.HasWhiteSpaces => true;

        string IFormatItem.WhiteSpaces => Item.SourcePart.Id;

    }
}