using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;

namespace hw.Parser
{
    public interface IParser<out TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        TSourcePart Execute(SourcePosn start);
        bool Trace { get; set; }
    }

    public interface IPriorityParser<TSourcePart>: IParser<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        TSourcePart Execute(SourcePosn start, Stack<OpenItem<TSourcePart>> stack );
    }

    public interface ISubParser<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        IPriorityParserTokenType<TSourcePart> Execute(SourcePosn sourcePosn, Stack<OpenItem<TSourcePart>> stack = null);
    }
}