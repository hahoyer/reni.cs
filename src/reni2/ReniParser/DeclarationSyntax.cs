using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Parser;
using hw.Scanner;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationSyntax : Syntax
    {
        internal DeclarationSyntax
            (
            Token token,
            CompileSyntax body,
            DefinableTokenSyntax target = null
            )
            : base(token)
        {
            Target = target;
            Body = body;
        }
        protected override ParsedSyntax[] Children => new ParsedSyntax[] {Target, Body};

        [EnableDump]
        DefinableTokenSyntax Target { get; }
        [EnableDump]
        Syntax Body { get; }
        [DisableDump]
        internal override bool IsMutableSyntax => Target?.IsMutable??false;
        [DisableDump]
        internal override bool IsConverterSyntax => Target?.IsConverter??false;
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => ToContainer;
        [DisableDump]
        internal override CompileSyntax ContainerStatementToCompileSyntax
            => Body.ContainerStatementToCompileSyntax;

        protected override string GetNodeDump() => (Name ?? "") + ": " + Body.NodeDump;

        string Name => Target?.Definable?.Name;

        internal override IEnumerable<KeyValuePair<string, int>> GetDeclarations(int index)
        {
            if(Name != null)
                yield return new KeyValuePair<string, int>(Name, index);
            foreach(var result in Body.GetDeclarations(index))
                yield return result;
        }

        internal override IEnumerable<string> GetDeclarations()
        {
            if(Name == null)
                yield break;
            yield return Name;
        }
    }
}