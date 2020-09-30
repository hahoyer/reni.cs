using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    class WhiteItem : DumpableObject, IFormatItem
    {
        readonly IItem Item;

        public WhiteItem(IItem item)
        {
            Item = item;
            StopByObjectIds(401);
        }

        [EnableDump]
        ITokenClass IFormatItem.TokenClass => null;

        [EnableDump]
        SourcePart IFormatItem.Content => Item.SourcePart;

        [EnableDump]
        bool IFormatItem.IsEssential => Lexer.IsMultiLineComment(Item) || Lexer.IsLineComment(Item);

        bool IFormatItem.HasEssentialWhiteSpaces => Item.IsComment();

        string IFormatItem.WhiteSpaces => Item.SourcePart.Id;
    }
}