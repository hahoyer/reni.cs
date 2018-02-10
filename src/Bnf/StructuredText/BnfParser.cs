using hw.DebugFormatter;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText
{
    public sealed class BnfParser<TSourcePart> : DumpableObject, IParser<TSourcePart>
        where TSourcePart : class, ISourcePartProxy
    {
        readonly IScanner Scanner;

        public BnfParser(IScanner scanner)
        {
            Scanner = scanner;
        }

        public bool Trace {get; set;}

        TSourcePart IParser<TSourcePart>.Execute(SourcePosn start)
        {
            var items = Scanner.GetNextTokenGroup(start);

            var prefixItems = items.PrefixItems;
            var mainItem = items.Characters;

            var parserType = items.Type;
/*            return new Item<TTreeItem>            (                prefixItems,                parserType,                mainItem.SourcePart,                context,                isBracketAndLeftBracket);  */
            NotImplementedMethod(start);
            return null;
        }
    }
}