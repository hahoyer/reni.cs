using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.Formatting;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class ExclamationBoxToken : DumpableObject, IType<SourceSyntax>, ITokenClass
    {
        SourceSyntax Value { get; }

        internal ExclamationBoxToken(SourceSyntax value) { Value = value; }

        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            Tracer.Assert(right == null);

            var result = Value.Syntax.ExclamationSyntax(token.Characters);

            if(left != null)
            {
                var leftResult = left.Syntax.Combine(result.Value);
                result = new Checked<ExclamationSyntaxList>
                    (leftResult.Value, leftResult.Issues.plus(result.Issues));
            }

            return new SourceSyntax(left, this, token, Value, result.Value, result.Issues);
        }

        string IType<SourceSyntax>.PrioTableId => PrioTable.Any;

        IType<SourceSyntax> IType<SourceSyntax>.NextTypeIfMatched => null;
    }
}