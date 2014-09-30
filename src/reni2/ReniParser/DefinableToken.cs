using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DefinableToken : DumpableObject, IIconKeyProvider
    {
        readonly TokenData _data;
        readonly Definable _tokenClass;

        internal DefinableToken(Definable tokenClass, TokenData tokenData)
        {
            _data = tokenData;
            _tokenClass = tokenClass;
        }

        public TokenData Data { get { return _data; } }

        [Node]
        internal Definable TokenClass { get { return _tokenClass; } }

        [DisableDump]
        public string IconKey { get { return "Symbol"; } }

        protected override string GetNodeDump() { return Data.Name.Quote(); }
    }
}