using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class ExclamationBoxToken : DumpableObject, IType<SourceSyntax>
    {
        SourceSyntax Value { get; }

        internal ExclamationBoxToken(SourceSyntax value) { Value = value; }

        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            Tracer.Assert(right == null);
            var syntax = Value.Syntax.ExclamationSyntax(token.Characters);
            if(left != null)
            {
                var leftResult = left.Syntax.Combine(syntax.Value);
                syntax = new Checked<ExclamationSyntaxList>
                    (leftResult.Value, leftResult.Issues.plus<Issue>(syntax.Issues));
            }
            return new SourceSyntax
                (left,
                token,
                Value,
                syntax.Value,
                syntax.Issues);
        }

        string IType<SourceSyntax>.PrioTableId => PrioTable.Any;

        IType<SourceSyntax> IType<SourceSyntax>.NextTypeIfMatched => null;
    }
}