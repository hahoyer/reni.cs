using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;

namespace Reni.Parser
{
    internal interface IParsedSyntax : IIconKeyProvider
    {
        // Helper methods

        [IsDumpEnabled(false)]
        TokenData Token { get; }

        TokenData FirstToken { get; }
        TokenData LastToken { get; }

        string Dump();
        string DumpShort();
    }
}