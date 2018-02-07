using System;
using hw.Scanner;

namespace hw.Parser
{
    [Obsolete("... since 18.1. Use ParserTokenType.",true)]
    public abstract class CommonTokenType<TTreeItem>
        : ParserTokenType<TTreeItem>
        where TTreeItem : class, ISourcePartProxy {}
}