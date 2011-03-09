using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    [Serializable]
    internal sealed class DefineableToken : ReniObject, IIconKeyProvider
    {
        private readonly TokenData _data;
        private readonly Defineable _tokenClass;

        internal DefineableToken(Defineable tokenClass, TokenData tokenData)
        {
            _data = tokenData;
            _tokenClass = tokenClass;
        }

        public TokenData Data { get { return _data; } }

        [Node]
        internal Defineable TokenClass { get { return _tokenClass; } }

        [IsDumpEnabled(false)]
        public string IconKey { get { return "Symbol"; } }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return Data.Name.Quote() + "." + ObjectId; } }
    }
}