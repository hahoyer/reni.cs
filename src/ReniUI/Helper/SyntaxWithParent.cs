using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;

namespace ReniUI.Helper
{
    abstract class SyntaxWithParent<TResult> : TreeWithParentExtended<TResult, Syntax>
        where TResult : SyntaxWithParent<TResult>
    {
        protected SyntaxWithParent(Syntax flatItem, TResult parent)
            : base(flatItem, parent) { }

        [DisableDump]
        internal IToken Token => FlatItem.Token;

        [DisableDump]
        internal SourcePart SourcePart
        {
            get
            {
                var left = this.GetNodesFromLeftToRight().First(node => node.FlatItem!= null).SourcePart;
                var right= this.GetNodesFromRightToLeft().First(node => node?.FlatItem!= null).SourcePart;
                return left.Start.Span(right.End);
            }
        }


        [EnableDump]
        internal ITokenClass TokenClass => FlatItem.TokenClass;

        internal bool Contains(int current)
            => SourcePart.Position <= current && current < SourcePart.EndPosition;

        bool IsLeftSiteChild(TResult child)
            => child != null && child.Token.SourcePart().End <= Token.SourcePart().Start;

        bool IsRightSideChild(TResult child)
            => child != null && Token.SourcePart().End <= child.Token.SourcePart().Start;
    }
}