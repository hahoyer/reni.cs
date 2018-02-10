using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public abstract class PriorityParserTokenType<TTreeItem>
        : DumpableObject, IPriorityParserTokenType<TTreeItem>
        where TTreeItem : class, ISourcePartProxy
    {
        TTreeItem IPriorityParserTokenType<TTreeItem>.Create(TTreeItem left, IPrioParserToken token, TTreeItem right)
            => Create(left, token, right);

        string IUniqueIdProvider.Value => Id;

        public abstract string Id {get;}
        protected abstract TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);

        protected override string GetNodeDump() => base.GetNodeDump() + "(" + Id.Quote() + ")";
        public override string ToString() => base.ToString() + " Id=" + Id.Quote();
    }
}