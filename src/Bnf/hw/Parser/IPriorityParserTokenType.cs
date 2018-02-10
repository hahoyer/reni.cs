using hw.Helper;
using hw.Scanner;

namespace hw.Parser
{
    public interface IPriorityParserTokenType<TTreeItem>: ITokenType
        where TTreeItem : class, ISourcePartProxy
    {
        TTreeItem Create(TTreeItem left, IPrioParserToken token, TTreeItem right);
    }

    public interface IBracketMatch<TTreeItem>
        where TTreeItem : class, ISourcePartProxy
    {
        IPriorityParserTokenType<TTreeItem> Value {get;}
    }
}