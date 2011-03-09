using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.ReniParser.TokenClasses
{
    /// <summary>
    ///     Any non reseved token
    /// </summary>
    [Serializable]
    internal sealed class UserSymbol : Defineable
    {
        private UserSymbol(string name) { Name = name; }

        public static TokenClass Instance(string name) { return new UserSymbol(name); }
    }
}