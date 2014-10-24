using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.PrioParser;
using hw.Scanner;

namespace hw.Parser
{
    sealed class Position : Dumpable, IPosition<IParsedSyntax>
    {
        internal readonly SourcePosn SourcePosn;
        readonly Scanner _scanner;
        readonly ITokenFactory _tokenFactory;

        Position(SourcePosn sourcePosn, ITokenFactory tokenFactory, Scanner scanner)
        {
            SourcePosn = sourcePosn;
            _tokenFactory = tokenFactory;
            _scanner = scanner;
        }

        Item<IParsedSyntax> IPosition<IParsedSyntax>.GetItemAndAdvance(Stack<OpenItem<IParsedSyntax>> stack)
        {
            return _scanner.CreateToken(SourcePosn, _tokenFactory, stack);
        }
        
        IPart IPosition<IParsedSyntax>.Span(IPosition<IParsedSyntax> end) { return TokenData.Span(SourcePosn, end); }

        /// <summary>
        ///     Scans and parses source and creates the syntax tree
        /// </summary>
        public static IParsedSyntax Parse(Source source, ITokenFactory tokenFactory, Scanner scanner, Stack<OpenItem<IParsedSyntax>> stack = null)
        {
            return Parse(source+0, tokenFactory,scanner,stack);
        }

        /// <summary>
        ///     Scans and parses source and creates the syntax tree
        /// </summary>
        public static IParsedSyntax Parse(SourcePosn sourcePosn, ITokenFactory tokenFactory, Scanner scanner, Stack<OpenItem<IParsedSyntax>> stack = null)
        {
            var p = new Position(sourcePosn, tokenFactory, scanner);
            return p.Parse(tokenFactory.PrioTable, stack);
        }
    }
}