using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.ReniSyntax;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.ReniParser
{
    sealed class DeclarationSyntax : Syntax
    {
        readonly SyntaxError[] _issues;
        internal DeclarationSyntax
            (
            SyntaxError[] issues,
            CompileSyntax body,
            Definable target,
            params DeclarationTagToken[] tags)
        {
            _issues = issues;
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

        internal override bool IsKeyword => true;

        [DisableDump]
        internal override bool IsMutableSyntax
            => Tags.Any(item => item.DeclaresMutable);

        [DisableDump]
        internal override bool IsConverterSyntax
            => Tags.Any(item => item.DeclaresConverter);

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
                yield return Body;
                if(_issues == null)
                    yield break;
                foreach(var issue in _issues)
                    yield return issue;
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