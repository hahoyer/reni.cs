using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public interface IToken
    {
        [DisableDump]
        IEnumerable<LexerToken> PrecededWith {get;}

        [DisableDump]
        SourcePart Characters {get;}
    }

    public interface IPrioParserToken: IToken
    {
        [DisableDump]
        bool? IsBracketAndLeftBracket {get;}
    }

    static class TokenExtension
    {
        internal static SourcePart SourcePart(this IToken token)
            => token.PrecededWith.SourcePart() + token.Characters;
    }
}