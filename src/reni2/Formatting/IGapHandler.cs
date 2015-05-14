using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace Reni.Formatting
{
    interface IGapHandler
    {
        string StartGap
            (
            bool level,
            int indentLevel,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right
            );

        string Gap
            (
            int indentLevel,
            ITokenClass left,
            IEnumerable<WhiteSpaceToken> rightWhiteSpaces,
            ITokenClass right
            );

        string Indent(string tag, WhiteSpaceToken[] precededWith);

    }
}