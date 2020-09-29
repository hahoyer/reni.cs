using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Helper
{
    abstract class SyntaxWithParent<TResult> : TreeWithParentExtended<TResult, Syntax>, ITree<TResult>
        where TResult : SyntaxWithParent<TResult>
    {
        protected SyntaxWithParent(Syntax target, TResult parent)
            : base(target, parent) {}

        TResult ITree<TResult>.Left => Left;
        TResult ITree<TResult>.Right => Right;

        [DisableDump]
        internal IToken Token => Target.Token;

        [EnableDumpExcept(null)]
        internal IEnumerable<IItem> WhiteSpaces => Token.PrecededWith;

        [DisableDump]
        internal SourcePart SourcePart
        {
            get
            {
                var l = LeftMost.Token.SourcePart();
                var r = RightMost.Token.SourcePart();
                return l.Start.Span(r.End);
            }
        }

        [EnableDump]
        internal ITokenClass TokenClass => Target.TokenClass;

        internal bool Contains(int current)
            => SourcePart.Position <= current && current < SourcePart.EndPosition;

    }
}