using System;
using System.Collections.Generic;
using System.Linq;

namespace hw.Scanner
{
    public interface ITokenFactory
    {
        object BeginOfText { get; }
        IScannerTokenType EndOfText { get; }
        IScannerTokenType InvalidCharacterError { get; }
        LexerItem[] Classes { get; }
    }
}