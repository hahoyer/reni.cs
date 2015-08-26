using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class DeclarationSyntax : Syntax
    {
        internal static Checked<Syntax> Create
            (Syntax body, Definable target, Exclamation.Syntax[] tags)
        {
            var rightResult = body.ToCompiledSyntax;
            return new DeclarationSyntax
                (rightResult.Value, target, tags.Select(item => item.Tag.Tag).ToArray())
                .Issues(rightResult.Issues);
        }

        internal DeclarationSyntax
            (
            CompileSyntax body,
            Definable target,
            params DeclarationTagToken[] tags)
        {
            Target = target;
            Body = body;
            Tags = tags;
            StopByObjectIds();
        }

        [EnableDump]
        DeclarationTagToken[] Tags { get; }
        [EnableDump]
        Definable Target { get; }
        [EnableDump]
        Syntax Body { get; }

        string Name => Target?.Id;

        [DisableDump]
        internal override bool IsMutableSyntax
            => Tags.Any(item => item.DeclaresMutable);

        [DisableDump]
        internal override bool IsConverterSyntax
            => Tags.Any(item => item.DeclaresConverter);

        [DisableDump]
        internal override bool IsMixInSyntax
            => Tags.Any(item => item.DeclaresMixIn);

        [DisableDump]
        internal override Checked<CompileSyntax> ToCompiledSyntax
            => Checked<CompileSyntax>.From(ToContainer);

        [DisableDump]
        internal override Checked<CompileSyntax> ContainerStatementToCompileSyntax
            => Body.ContainerStatementToCompileSyntax;

        [DisableDump]
        protected override IEnumerable<Syntax> DirectChildren { get { yield return Body; } }

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

        internal override IEnumerable<Syntax> GetMixins(CompoundView context, int position)
            => IsMixInSyntax
                ? context.AccessType(position).GetMixins()
                : base.GetMixins(context, position);
    }
}