using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    class TokenItem : DumpableObject, IFormatItem
    {
        readonly bool IgnoreWhitespaces;
        readonly Syntax Syntax;

        public TokenItem(Syntax syntax, bool ignoreWhitespaces)
        {
            Syntax = syntax;
            IgnoreWhitespaces = ignoreWhitespaces;
        }

        [EnableDump]
        SourcePart IFormatItem.Content => Syntax.Main;

        [EnableDump]
        bool IFormatItem.IsEssential => true;

        [EnableDump]
        ITokenClass IFormatItem.TokenClass => Syntax.TokenClass;

        bool IFormatItem.HasEssentialWhiteSpaces
        {
            get
            {
                var tokenPrecededWith = Syntax.LeftWhiteSpaces;
                return !IgnoreWhitespaces && tokenPrecededWith.HasComment();
            }
        }

        string IFormatItem.WhiteSpaces => IgnoreWhitespaces ? "" : Syntax.LeftWhiteSpaces.SourcePart()?.Id ?? "";
    }
}