using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public sealed class OpenItem<TTreeItem> : DumpableObject
        where TTreeItem : class, ISourcePartProxy
    {
        internal readonly PrioTable.ITargetItem BracketItem;
        internal readonly TTreeItem Left;
        internal readonly IPrioParserToken Token;
        internal readonly IPriorityParserTokenType<TTreeItem> Type;

        internal OpenItem(TTreeItem left, Item<TTreeItem> current)
        {
            Left = left;
            Type = current.Type;
            Token = current;
            BracketItem = current;
        }

        internal int NextDepth => BracketItem.GetRightDepth();

        internal TTreeItem Create(TTreeItem right)
        {
            if(Type != null)
                return Type.Create(Left, Token, right);
            Tracer.Assert(Left == null);
            return right;
        }

        protected override string GetNodeDump()
            => Tracer.Dump(Left) + " " + Type.GetType().PrettyName();
    }
}