using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Parser;
using Reni.Formatting;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class ExclamationBoxToken : DumpableObject, IType<SourceSyntax>, ITokenClass
    {
        SourceSyntax Value { get; }

        internal ExclamationBoxToken(SourceSyntax value) { Value = value; }

        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            Tracer.Assert(right == null);
            return new SourceSyntax(left, this, token, Value, GetResult);
        }

        static Checked<ExclamationSyntaxList> GetResult(Syntax left, IToken token, Syntax right)
        {
            var result = right.ExclamationSyntax(token.Characters);
            if(left == null)
                return result;

            var leftResult = left.Combine(result.Value);
            return new Checked<ExclamationSyntaxList>
                (leftResult.Value, leftResult.Issues.plus(result.Issues));
        }

        string IType<SourceSyntax>.PrioTableId => PrioTable.Any;
        string ITokenClass.Id => "!";

    }
}