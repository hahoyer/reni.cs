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
        internal DeclarationSyntax
            (
            SourcePart token,
            CompileSyntax body,
            Definable definable = null,
            bool isConverter = false,
            bool isMutable = false)
            : base(token+body?.SourcePart, token)
        {
            Definable = definable;
            IsConverter = isConverter;
            IsMutable = isMutable;
            Body = body;
        }

        [EnableDump]
        Definable Definable { get; }
        [EnableDump]
        bool IsConverter { get; }
        [EnableDump]
        bool IsMutable { get; }
        [EnableDump]
        Syntax Body { get; }
        [DisableDump]
        internal override bool IsMutableSyntax => IsMutable;
        internal override bool IsConverterSyntax => IsConverter;
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => ToContainer;
        [DisableDump]
        internal override CompileSyntax ContainerStatementToCompileSyntax
            => Body.ContainerStatementToCompileSyntax;

        protected override string GetNodeDump() => (Definable?.Name ?? "") + ": " + Body.NodeDump;

        internal override IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            yield return new KeyValuePair<string, int>(Definable.Name, index);
            foreach(var result in Body.GetDeclarations(index))
                yield return result;
        }

        internal override IEnumerable<string> GetDeclarations()
        {
            if(Definable == null)
                yield break;
            yield return Definable.Name;
        }
    }
}