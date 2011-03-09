using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Struct;

namespace Reni.Parser
{
    internal class ParsedSyntax : ReniObject, IParsedSyntax
    {
        private static bool _isInDump;

        [UsedImplicitly]
        internal static bool IsDetailedDumpRequired = true;

        private readonly TokenData _token;

        protected ParsedSyntax(TokenData token) { _token = token; }
        protected ParsedSyntax(TokenData token, int nextObjectId)
            : base(nextObjectId)
        {
            _token = token;
        }


        [IsDumpEnabled(false)]
        string IIconKeyProvider.IconKey { get { return "Syntax"; } }

        string IParsedSyntax.DumpShort() { return DumpShort(); }

        TokenData IParsedSyntax.Token { get { return Token; } }
        TokenData IParsedSyntax.FirstToken { get { return GetFirstToken(); } }
        TokenData IParsedSyntax.LastToken { get { return GetLastToken(); } }

        internal TokenData Token { get { return _token; } }
        internal TokenData FirstToken { get { return GetFirstToken(); } }
        internal TokenData LastToken { get { return GetLastToken(); } }

        protected virtual TokenData GetFirstToken() { return Token; }
        protected virtual TokenData GetLastToken() { return Token; }

        internal virtual string DumpShort() { return Token.Name; }
        protected virtual string FilePosition() { return Token.FilePosition; }

        protected override sealed string Dump(bool isRecursion)
        {
            if (isRecursion)
                return "ObjectId=" + ObjectId;

            var isInContainerDump = Container.IsInContainerDump;
            Container.IsInContainerDump = false;
            var isInDump = _isInDump;
            _isInDump = true;
            var result = DumpShort();
            if (!IsDetailedDumpRequired)
                return result;
            if (!isInDump)
                result += FilePosition();
            if (!isInContainerDump)
                result += "\n" + base.Dump(isRecursion);
            Container.IsInContainerDump = isInContainerDump;
            _isInDump = isInDump;
            return result;
        }

        [IsDumpEnabled(false), UsedImplicitly]
        public new string NodeDump { get { return base.NodeDump + " " + DumpShort(); } }

    }
}