using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class ExclamationBoxToken : DumpableObject, IType<SourceSyntax>
    {
        SourceSyntax Value { get; }

        internal ExclamationBoxToken(SourceSyntax value) { Value = value; }

        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            Tracer.Assert(right == null);
            ExclamationSyntaxList syntax = Value.Syntax.ExclamationSyntax(token.Characters);
            if(left != null)
                syntax = left.Syntax.ExclamationSyntax(syntax);
            return new SourceSyntax(syntax, left, token, Value);
        }

        string IType<SourceSyntax>.PrioTableId => PrioTable.Any;

        IType<SourceSyntax> IType<SourceSyntax>.NextTypeIfMatched => null;
    }
}