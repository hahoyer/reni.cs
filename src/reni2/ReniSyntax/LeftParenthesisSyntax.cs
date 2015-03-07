using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
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
            (int parenthesis, SourcePart token, Syntax right)
            : base(token + right?.SourcePart, token)
        {
            _parenthesis = parenthesis;
            _right = right;
        }

        internal override Syntax RightParenthesis(int level, SourcePart token)
        {
            Tracer.Assert(level == _parenthesis);
            if(_right == null)
                return new EmptyList(SourcePart, token);
            return new SourroundSyntax(SourcePart + token, _right, token);
        }
    }

    sealed class SourroundSyntax : Syntax
    {
        readonly Syntax _right;
        public SourroundSyntax(SourcePart sourcePart, Syntax right, SourcePart token)
            : base(sourcePart, token) { _right = right; }
        internal override CompileSyntax ToCompiledSyntax
            => _right.ToCompiledSyntax.Sourround(SourcePart + Token);
    }
}