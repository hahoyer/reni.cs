using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses {
    class TokenItem : DumpableObject, IFormatItem
    {
        readonly Syntax Syntax;
        readonly bool IgnoreWhitespaces;

        public TokenItem(Syntax syntax, bool ignoreWhitespaces)
        {
            Syntax = syntax;
            IgnoreWhitespaces = ignoreWhitespaces;
        }

        [EnableDump]
        SourcePart IFormatItem.Content => Syntax.Token.Characters;

        [EnableDump]
        bool IFormatItem.IsEssential => true;

        [EnableDump]
        ITokenClass IFormatItem.TokenClass => Syntax.TokenClass;

        [EnableDump]
        bool IFormatItem.HasWhiteSpaces => !IgnoreWhitespaces && Syntax.Token.PrecededWith.Any();

        string IFormatItem.WhiteSpaces => Syntax.Token.PrecededWith.SourcePart().Id;
    }
}