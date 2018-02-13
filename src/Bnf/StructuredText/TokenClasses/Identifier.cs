using System.Collections.Generic;
using Bnf.Forms;
using Bnf.Parser;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;

namespace Bnf.StructuredText.TokenClasses
{
    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class Identifier : DumpableObject, ITokenType, IDeclaration<ISyntax>
    {
        const string Name = "identifier";

        ISyntax IDeclaration<ISyntax>.Parse(IParserCursor source, IContext<ISyntax> context)
        {
            var token = context[source];
            if(token.Type == this)
                return new Singleton(token);
            NotImplementedMethod(source, context);
            return null;
        }

        string IDeclaration<ISyntax>.Name => Name;

        string IUniqueIdProvider.Value => Name;

        IEnumerable<ITerminal> IDeclaration<ISyntax>.Terminals
        {
            get {yield break;}
        }

    }
}