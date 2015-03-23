using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class SyntaxBoxToken : TerminalToken
    {
        readonly SourceSyntax _value;
        public SyntaxBoxToken(SourceSyntax value) { _value = value; }

        protected override ReniParser.Syntax Terminal(SourcePart token) => new Syntax(_value);
        public override string Id => "<box>";

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
    }
}