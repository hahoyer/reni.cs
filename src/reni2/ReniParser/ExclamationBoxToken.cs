using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class ExclamationBoxToken : DumpableObject, IType<SourceSyntax>
    {
        internal readonly SourceSyntax Value;
        public ExclamationBoxToken(SourceSyntax value) { Value = value; }

        internal sealed class Syntax : ReniParser.Syntax
        {
            public Syntax(SourceSyntax value) { Value = value; }

            internal SourceSyntax Value { get; }

            [DisableDump]
            internal override CompileSyntax ToCompiledSyntax
            {
                get
                {
                    NotImplementedMethod();
                    return null;
                }
            }
        }

        SourceSyntax IType<SourceSyntax>.Create(SourceSyntax left, IToken token, SourceSyntax right)
        {
            Tracer.Assert(right == null);
            return new SourceSyntax(Value.Syntax.ExclamationSyntax(token.Characters), left, token, Value);
        }

        string IType<SourceSyntax>.PrioTableId => PrioTable.Any;

        IType<SourceSyntax> IType<SourceSyntax>.NextTypeIfMatched => null;
    }
}