using System;
using System.Collections.Generic;
using System.Linq;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationSyntax : Syntax
    {
        internal readonly Definable Definable;
        internal readonly Syntax Body;

        internal DeclarationSyntax(Definable definable, SourcePart token, Syntax body)
            : base(token)
        {
            Definable = definable;
            Body = body;
        }

        protected override string GetNodeDump() { return Definable.Name + ": " + Body.NodeDump; }

        internal override CompileSyntax ContainerStatementToCompileSyntax { get { return Body.ContainerStatementToCompileSyntax; } }

        internal override IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield return new KeyValuePair<string, int>(Definable.Name, index);
            foreach(var result in Body.GetDeclarations(index))
                yield return result;
        }

        internal override IEnumerable<string> GetDeclarations()
        {
            yield return Definable.Name;
            foreach (var result in Body.GetDeclarations())
                yield return result;
        }
        internal override bool IsEnableReassignSyntax { get { return Body.IsEnableReassignSyntax; } }
    }
}