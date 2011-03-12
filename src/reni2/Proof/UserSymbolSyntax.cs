using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Helper;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class UserSymbolSyntax : ParsedSyntax
    {
        internal readonly string Name;

        public UserSymbolSyntax(TokenData token, string name)
            : base(token) { Name = name; }

        internal override bool IsDistinct(ParsedSyntax other) { return IsDistinct((UserSymbolSyntax) other); }
        private bool IsDistinct(UserSymbolSyntax other) { return Name != other.Name; }
        internal override Set<string> Variables { get { return new Set<string>(){Name}; } }
    }
}