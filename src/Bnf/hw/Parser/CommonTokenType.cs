using System;
using hw.Scanner;

namespace hw.Parser
{
    [Obsolete("... since 18.1. Use PriorityParserTokenType.",true)]
    public abstract class CommonTokenType<TTreeItem>
        : PriorityParserTokenType<TTreeItem>
        where TTreeItem : class, ISourcePartProxy {}
}