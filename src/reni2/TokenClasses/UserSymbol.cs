using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Parser;

namespace Reni.TokenClasses
{
    sealed class UserSymbol : Definable
    {
        internal UserSymbol(string name) { Id = name; }
        public override string Id { get; }
    }
}