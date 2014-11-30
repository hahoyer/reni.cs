using System.Collections.Generic;
using System.Linq;
using System;

namespace Reni.TokenClasses
{
    sealed class ReassignToken : Definable
    {
        public const string Id = ":=";
    }

    sealed class EnableReassignToken : TokenClass
    {
        public const string Id = "<:=>";
    }
}