using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;

namespace Reni.Helper
{
    abstract class BinaryTreeSyntaxWithParent<TResult> : TreeWithParentExtended<TResult, BinaryTree>
        where TResult : BinaryTreeSyntaxWithParent<TResult>
    {
        protected BinaryTreeSyntaxWithParent(BinaryTree target, TResult parent)
            : base(target, parent) { }

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
        [EnableDumpExcept(null)]
        public TResult Left => ((ITree<TResult>)this).GetDirectNode(0);

        [EnableDump]
        [EnableDumpExcept(null)]
        public TResult Right => ((ITree<TResult>)this).GetDirectNode(2);

        [EnableDump]
        internal ITokenClass TokenClass => Target.TokenClass;

        internal bool Contains(int current)
            => SourcePart.Position <= current && current < SourcePart.EndPosition;
    }
}