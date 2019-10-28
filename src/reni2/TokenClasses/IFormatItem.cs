using hw.DebugFormatter;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses {
    interface IFormatItem
    {
        [EnableDump]
        SourcePart Content {get;}

        [EnableDump]
        bool IsEssential {get;}

        [EnableDump]
        ITokenClass TokenClass {get;}

        [EnableDump]
        bool HasEssentialWhiteSpaces {get;}

        [EnableDump]
        string WhiteSpaces {get;}
    }
}