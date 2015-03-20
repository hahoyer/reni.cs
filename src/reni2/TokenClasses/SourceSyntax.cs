using System;
using System.Collections.Generic;
using System.Linq;
using hw.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    sealed class SourceSyntax : ParsedSyntax
    {
        public SourceSyntax(Syntax syntax, IToken token)
            : base(token) { Syntax = syntax; }
        public Syntax Syntax { get; }
    }
}