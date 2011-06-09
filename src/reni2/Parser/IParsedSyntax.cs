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

        [DisableDump]
        TokenData Token { get; }

        TokenData FirstToken { get; }
        TokenData LastToken { get; }

        string Dump();
        string DumpShort();
    }
}