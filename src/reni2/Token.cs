using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni
{
    public sealed class Token : DumpableObject
    {
        internal Token(Syntax syntax) { Syntax = syntax; }

        Syntax Syntax { get; }
        public SourcePosn Start => Syntax.SourcePart.Start;
        public SourcePosn End => Syntax.SourcePart.End;
        public bool IsKeyword => Syntax.IsKeyword;
        public bool IsIdentifier => Syntax.IsIdentifier;
        public bool IsText => Syntax.IsText;
        public bool IsNumber => Syntax.IsNumber;
        public bool IsComment => false;
        public bool IsLineComment => false;
        public bool IsWhiteSpace => false;
        public bool IsBraceLike => false;
    }
}