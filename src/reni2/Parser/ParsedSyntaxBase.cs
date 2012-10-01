using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;

namespace Reni.Parser
{
    internal class ParsedSyntaxBase : ReniObject, IParsedSyntax
    {
        [UsedImplicitly]
        internal static bool IsDetailedDumpRequired = true;

        private readonly TokenData _token;

        protected ParsedSyntaxBase(TokenData token) { _token = token; }

        protected ParsedSyntaxBase(TokenData token, int nextObjectId)
            : base(nextObjectId) { _token = token; }


        [DisableDump]
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }

        string IParsedSyntax.DumpShort() { return GetNodeDump(); }

        [DisableDump]
        TokenData IParsedSyntax.Token { get { return Token; } }

        [DisableDump]
        TokenData IParsedSyntax.FirstToken { get { return GetFirstToken(); } }

        [DisableDump]
        TokenData IParsedSyntax.LastToken { get { return GetLastToken(); } }

        [DisableDump]
        internal TokenData Token { get { return _token; } }

        [DisableDump]
        internal TokenData FirstToken { get { return GetFirstToken(); } }

        [DisableDump]
        internal TokenData LastToken { get { return GetLastToken(); } }

        protected virtual TokenData GetFirstToken() { return Token; }
        protected virtual TokenData GetLastToken() { return Token; }

        protected override string GetNodeDump() { return Token.Name; }
        protected virtual string FilePosition() { return Token.FilePosition; }
    }
}