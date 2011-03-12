using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Proof
{
    internal sealed class LeftParenthesisSyntax : ParsedSyntax
    {
        internal readonly int Level;
        internal readonly ParsedSyntax Right;

        public LeftParenthesisSyntax(int level, TokenData token, ParsedSyntax right)
            : base(token)
        {
            Level = level;
            Right = right;
        }
    }
}