using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.ReniSyntax;

namespace Reni.TokenClasses
{
    sealed class DefinableTokenSyntax : Syntax
    {
        readonly Definable _definable;

        internal DefinableTokenSyntax(Definable definable, SourcePart tokenData)
            : base(tokenData)
        {
            _definable = definable;
        }

        internal override Syntax CreateDeclarationSyntax(SourcePart token, Syntax right)
        {
            return new DeclarationSyntax(_definable, token, right);
        }
        internal override Syntax SurroundedByParenthesis(SourcePart leftToken, SourcePart rightToken) { return this; }
        internal override CompileSyntax ToCompiledSyntax { get { return new ExpressionSyntax(_definable, null, Token, null); } }
    }
}