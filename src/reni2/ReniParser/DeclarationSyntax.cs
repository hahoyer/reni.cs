using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.ReniSyntax;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationSyntax : Syntax
    {
        internal DeclarationSyntax
            (CompileSyntax body, DefinableSyntax target, DeclarationTagToken tag = null)
        {
            Target = target;
            Body = body;
            Tag = tag;
            StopByObjectIds();
        }

        [EnableDump]
        DeclarationTagToken Tag { get; }
        [EnableDump]
        DefinableSyntax Target { get; }
        [EnableDump]
        Syntax Body { get; }

        string Name => Target?.Definable?.Id;

        internal override bool IsKeyword => true;

        [DisableDump]
        internal override bool IsMutableSyntax
            => Tag?.DeclaresMutable ?? base.IsMutableSyntax;

        [DisableDump]
        internal override bool IsConverterSyntax
            => Tag?.DeclaresConverter ?? base.IsConverterSyntax;

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