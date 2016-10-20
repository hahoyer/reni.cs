using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken : DumpableObject,
        IParserTokenType<Syntax>,
        ITokenClass,
        IDeclaratorTokenClass
    {
        Syntax Value { get; }

        internal ExclamationBoxToken(Syntax value) { Value = value; }

        Syntax IParserTokenType<Syntax>.Create(Syntax left, IToken token, Syntax right)
        {
            Tracer.Assert(right == null);
            return Syntax.CreateSourceSyntax(left, this, token, Value);
        }

        string IParserTokenType<Syntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";

        Result<Declarator> IDeclaratorTokenClass.Get(Syntax syntax)
        {
            if(syntax.Left != null || syntax.Right == null)
            {
                NotImplementedMethod(syntax);
                return null;
            }

            var provider = syntax.Right.TokenClass as IDeclaratorTagProvider;
            if (provider == null)
            {
                NotImplementedMethod(syntax);
                return null;
            }

            return provider.Get(syntax.Right);
        }
    }
}