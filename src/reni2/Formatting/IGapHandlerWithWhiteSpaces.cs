using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;

namespace Reni.Formatting
{
    internal interface IGapHandlerWithWhiteSpaces
    {
        string StartGap(IEnumerable<WhiteSpaceToken> rightWhiteSpaces, ITokenClass right);

        string Gap
            (ITokenClass left, IEnumerable<WhiteSpaceToken> rightWhiteSpaces, ITokenClass right);
    }
}