using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationSyntax : Syntax
    {
        internal readonly Definable Definable;
        internal readonly Syntax Definition;

        internal DeclarationSyntax(Definable definable, SourcePart token, Syntax definition)
            : base(token)
        {
            Definable = definable;
            Definition = definition;
        }

        protected override string GetNodeDump() { return Definable.Name + ": " + Definition.NodeDump; }

        internal override Syntax SurroundedByParenthesis(SourcePart leftToken, SourcePart rightToken)
        {
            return Container.Create(leftToken, rightToken, this);
        }
    }
}