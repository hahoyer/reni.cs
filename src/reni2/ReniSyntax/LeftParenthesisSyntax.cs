using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniParser;

namespace Reni.ReniSyntax
{
    sealed class LeftParenthesisSyntax : Syntax
    {
        [EnableDump]
        readonly int _parenthesis;
        [EnableDump]
        readonly Syntax _right;

        public LeftParenthesisSyntax
            (int parenthesis, Token token, Syntax right)
            : base(token)
        {
            _parenthesis = parenthesis;
            _right = right;
        }

        internal override Syntax RightParenthesis(int level, Token token)
        {
            Tracer.Assert(level == _parenthesis);
            if(_right == null)
                return new EmptyList(token, SourcePart);
            return new SourroundSyntax(_right, SourcePart + token.SourcePart);
        }
        [DisableDump]
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {_right};
    }

    sealed class SourroundSyntax : Syntax
    {
        readonly Syntax _right;
        public SourroundSyntax(Syntax right, SourcePart sourcePart)
            : base(right.Token, sourcePart) { _right = right; }
        internal override CompileSyntax ToCompiledSyntax
            => _right.ToCompiledSyntax.Sourround(SourcePart);
    }
}