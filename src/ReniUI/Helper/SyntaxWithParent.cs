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
        internal IToken Token => Target.Target.Token;

        [DisableDump]
        internal SourcePart SourcePart
        {
            get
            {
                var left = this.GetNodesFromLeftToRight().First(node => node?.Target.Target != null).Token.SourcePart();
                var right= this.GetNodesFromRightToLeft().First(node => node?.Target.Target != null).Token.SourcePart();
                return left.Start.Span(right.End);
            }
        }


        [EnableDump]
        internal ITokenClass TokenClass => Target.Target.TokenClass;

        internal bool Contains(int current)
            => SourcePart.Position <= current && current < SourcePart.EndPosition;

        bool IsLeftSiteChild(TResult child)
            => child != null && child.Token.SourcePart().End <= Token.SourcePart().Start;

        bool IsRightSideChild(TResult child)
            => child != null && Token.SourcePart().End <= child.Token.SourcePart().Start;
    }
}