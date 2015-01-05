using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationSyntax : Syntax
    {
        internal DeclarationSyntax(Definable definable, SourcePart token, Syntax body)
            : base(token)
        {
            Definable = definable;
            Body = body;
        }

        [EnableDump]
        Definable Definable { get; }
        [EnableDump]
        Syntax Body { get; }
        [DisableDump]
        internal override bool IsEnableReassignSyntax => Body.IsEnableReassignSyntax;
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => ToContainer;
        [DisableDump]
        internal override CompileSyntax ContainerStatementToCompileSyntax => Body.ContainerStatementToCompileSyntax;

        protected override string GetNodeDump() => Definable.Name + ": " + Body.NodeDump;

        internal override IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield return new KeyValuePair<string, int>(Definable.Name, index);
            foreach(var result in Body.GetDeclarations(index))
                yield return result;
        }

        internal override IEnumerable<string> GetDeclarations()
        {
            yield return Definable.Name;
            foreach(var result in Body.GetDeclarations())
                yield return result;
        }
    }
}