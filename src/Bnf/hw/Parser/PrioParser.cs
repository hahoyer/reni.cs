using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Scanner;

namespace hw.Parser
{
    public sealed partial class PrioParser<TSourcePart> : DumpableObject, IPriorityParser<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        readonly PrioTable PrioTable;
        readonly IScanner Scanner;
        readonly IPriorityParserTokenType<TSourcePart> StartParserType;

        public PrioParser(PrioTable prioTable, IScanner scanner, IPriorityParserTokenType<TSourcePart> startParserType)
        {
            PrioTable = prioTable;
            Scanner = scanner;
            StartParserType = startParserType;
        }

        TSourcePart IParser<TSourcePart>.Execute(SourcePosn start) => Execute(start, null);
        public bool Trace {get; set;}

        TSourcePart IPriorityParser<TSourcePart>.Execute(SourcePosn start, Stack<OpenItem<TSourcePart>> initialStack) 
            => Execute(start, initialStack);

        TSourcePart Execute(SourcePosn start, Stack<OpenItem<TSourcePart>> initialStack)
        {
            StartMethodDump(Trace, start.GetDumpAroundCurrent(50), initialStack);
            try
            {
                return ReturnMethodDump(CreateWorker(initialStack).Execute(start));
            }
            finally
            {
                EndMethodDump();
            }
        }

        PrioTable.Relation GetRelation(PrioTable.ITargetItem newType, PrioTable.ITargetItem topType)
            => PrioTable.GetRelation(newType, topType);

        PrioParserWorker CreateWorker(Stack<OpenItem<TSourcePart>> stack)
            => new PrioParserWorker(this, stack);
    }
}