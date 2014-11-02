using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniParser;
using Reni.Struct;

namespace Reni.ReniSyntax
{
    sealed class ConverterSyntax : Syntax
    {
        internal readonly CompileSyntax Body;

        internal ConverterSyntax(SourcePart token, CompileSyntax body)
            : base(token)
        {
            Body = body;
        }

        protected override string GetNodeDump() { return "converter (" + Body.NodeDump + ")"; }

        internal override Syntax RightParenthesis(int level, SourcePart token)
        {
            if(level == 0)
                return Container.Create(token, this);

            return base.RightParenthesis(level, token);
        }
        internal override Syntax RightParenthesisOnRight(int level, SourcePart rightToken)
        {
            return Container.Create(Token, (Syntax) this);
        }
        internal override Syntax SurroundedByParenthesis(SourcePart leftToken, SourcePart rightToken)
        {
            return Container.Create(leftToken, (Syntax) this);
        }
    }
}