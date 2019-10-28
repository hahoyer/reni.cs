using System;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    class WhiteItem : DumpableObject, IFormatItem
    {
        readonly IItem Item;
        readonly Syntax Syntax;

        public WhiteItem(IItem item, Syntax syntax)
        {
            Item = item;
            Syntax = syntax;
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