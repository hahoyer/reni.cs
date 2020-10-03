using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;

namespace ReniUI.Helper
{
    abstract class SyntaxWithParent<TResult> : TreeWithParentExtended<TResult, Reni.Parser.Syntax>
        where TResult : SyntaxWithParent<TResult>
    {
        protected SyntaxWithParent(Reni.Parser.Syntax target, TResult parent)
            : base(target, parent) { }

        [DisableDump]
        internal IToken Token => Target.BinaryTree.Token;

        [DisableDump]
        internal TResult FirstOrDefault => Children.FirstOrDefault(IsLeftSiteChild);

        [DisableDump]
        internal TResult LastOrDefault => Children.LastOrDefault(IsRightSideChild);

        [DisableDump]
        internal IEnumerable<TResult> LeftChildren => Children.Where(IsLeftSiteChild);

        [DisableDump]
        internal IEnumerable<TResult> RightChildren => Children.Where(IsRightSideChild);

        [DisableDump]
        internal TResult LeftMost => FirstOrDefault?.LeftMost ?? (TResult)this;

        [DisableDump]
        internal TResult RightMost => LastOrDefault?.RightMost ?? (TResult)this;

        [DisableDump]
        internal TResult LeftNeighbor => FirstOrDefault?.RightMost ?? LeftParent;

        [DisableDump]
        internal TResult RightNeighbor => LastOrDefault?.LeftMost ?? RightParent;

        [DisableDump]
        internal bool IsLeftChild => Parent?.FirstOrDefault == this;

        [DisableDump]
        internal bool IsRightChild => Parent?.FirstOrDefault == this;

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
        internal ITokenClass TokenClass => Target.BinaryTree.TokenClass;

        [DisableDump]
        TResult LeftParent
            => Parent != null && Parent.FirstOrDefault?.Target == Target
                ? Parent.LeftParent
                : Parent;

        [DisableDump]
        TResult RightParent
            => Parent != null && Parent.LastOrDefault?.Target == Target
                ? Parent.RightParent
                : Parent;

        internal bool Contains(int current)
            => SourcePart.Position <= current && current < SourcePart.EndPosition;

        bool IsLeftSiteChild(TResult child)
            => child != null && child.Token.SourcePart().End <= Token.SourcePart().Start;

        bool IsRightSideChild(TResult child)
            => child != null && Token.SourcePart().End <= child.Token.SourcePart().Start;
    }
}