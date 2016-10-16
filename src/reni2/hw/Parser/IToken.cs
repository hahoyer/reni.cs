using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public interface IToken
    {
        [DisableDump]
        IEnumerable<IItem> PrecededWith { get; }
        [DisableDump]
        SourcePart Characters { get; }
        [DisableDump]
        bool? IsBracketAndLeftBracket { get; }
    }

    static class TokenExtension
    {
        internal static SourcePart SourcePart(this IToken token)
            => token.PrecededWith.SourcePart() + token.Characters;
    }
}