using System;
using System.Collections.Generic;
using hw.Helper;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DefinableToken : DumpableObject, IIconKeyProvider
    {
        readonly SourcePart _data;
        readonly Definable _tokenClass;

        internal DefinableToken(Definable tokenClass, SourcePart tokenData)
        {
            _data = tokenData;
            _tokenClass = tokenClass;
        }

        public SourcePart Data => _data;

        [Node]
        internal Definable TokenClass => _tokenClass;

        [DisableDump]
        public string IconKey => "Symbol";

        protected override string GetNodeDump() => Data.Id.Quote();
    }
}