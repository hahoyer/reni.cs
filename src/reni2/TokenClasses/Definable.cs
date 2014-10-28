using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    abstract class Definable : TokenClass
    {
        protected override sealed Syntax TerminalSyntax(SourcePart token) { return new DefinableTokenSyntax(this, token); }
        protected override sealed Syntax PrefixSyntax(SourcePart token, Syntax right)
        {
            return new ExpressionSyntax(this, null, token, (CompileSyntax) right);
        }
        protected override sealed Syntax SuffixSyntax(Syntax left, SourcePart token)
        {
            return left.CreateSyntaxOrDeclaration(this, token, null);
        }
        protected override sealed Syntax Infix(Syntax left, SourcePart token, Syntax right)
        {
            return left.CreateSyntaxOrDeclaration(this, token, right);
        }

        [DisableDump]
        protected string DataFunctionName { get { return Name.Symbolize(); } }

        [DisableDump]
        internal virtual IEnumerable<IGenericProviderForDefinable> Genericize { get { return this.GenericListFromDefinable(); } }
    }

    sealed class ConcatArrays : Definable
    {
        [DisableDump]
        internal override IEnumerable<IGenericProviderForDefinable> Genericize
        {
            get { return this.GenericListFromDefinable(base.Genericize); }
        }
    }
}