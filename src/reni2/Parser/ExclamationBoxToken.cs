using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using JetBrains.Annotations;
using Reni.TokenClasses;
using Reni.Validation;

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

        [CanBeNull]
        Result<Declarator> IDeclaratorTokenClass.Get(Syntax syntax)
        {
            var provider = syntax.Right.TokenClass as IDeclaratorTagProvider;
            if(provider == null)
                return new Declarator(null, null, syntax.SourcePart)
                    .Issues(IssueId.UnknownDeclarationTag.Create(syntax.SourcePart));

            var result = provider.Get(syntax.Right);

            var other = syntax.Left?.Declarator;
            if(other == null)
                return result;

            return result.Target.Combine(other.Target).Issues(result.Issues.plus(other.Issues));
        }
    }
}