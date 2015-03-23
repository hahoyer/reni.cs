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
            SourcePart token,
            CompileSyntax body,
            DefinableSyntax target = null
            )
            : base()
        {
            Target = target;
            Body = body;
            StopByObjectIds();
        }

        [EnableDump]
        DefinableSyntax Target { get; }
        [EnableDump]
        Syntax Body { get; }

        string Name => Target?.Definable?.Id;
        internal override bool IsKeyword => true;
        [DisableDump]
        internal override bool IsMutableSyntax => Target?.IsMutable ?? false;
        [DisableDump]
        internal override bool IsConverterSyntax => Target?.IsConverter ?? false;
        [DisableDump]
        internal override CompileSyntax ToCompiledSyntax => ToContainer;
        [DisableDump]
        internal override CompileSyntax ContainerStatementToCompileSyntax
            => Body.ContainerStatementToCompileSyntax;

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren
        {
            get
            {
                yield return Target;
                yield return Body;
            }
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() + 
            (Name == null ? "" : "(" + Name + ")");


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