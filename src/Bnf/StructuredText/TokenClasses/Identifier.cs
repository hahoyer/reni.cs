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

        IEnumerable<IExpression> IDeclaration<ISyntax>.Items {get {yield break;}}

        string IUniqueIdProvider.Value => Name;
    }

    [BelongsTo(typeof(ScannerTokenFactory))]
    [BelongsTo(typeof(Compiler))]
    sealed class SignedInteger : DumpableObject, ITokenType, IDeclaration<ISyntax>
    {
        const string Name = "signed_integer";

        ISyntax IDeclaration<ISyntax>.Parse(IParserCursor source, IContext<ISyntax> context)
        {
            NotImplementedMethod(source, context);
            return null;
        }

        string IDeclaration<ISyntax>.Name => Name;

        IEnumerable<IExpression> IDeclaration<ISyntax>.Items {get {yield break;}}

        string IUniqueIdProvider.Value => Name;
    }
}