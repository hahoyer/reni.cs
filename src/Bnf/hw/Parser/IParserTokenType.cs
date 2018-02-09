using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IParserTokenType<TTreeItem>
        where TTreeItem : class, ISourcePartProxy
    {
        TTreeItem Create(TTreeItem left, IToken token, TTreeItem right);
        string Id { get; }
    }

    public interface IBracketMatch<TTreeItem>
        where TTreeItem : class, ISourcePartProxy
    {
        IParserTokenType<TTreeItem> Value { get; }
    }

}