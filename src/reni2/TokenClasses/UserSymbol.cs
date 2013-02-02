using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.TokenClasses
{
    internal sealed class UserSymbol : Defineable<UserSymbol>
    {
        internal UserSymbol(string name) { Name = name; }
    }
}