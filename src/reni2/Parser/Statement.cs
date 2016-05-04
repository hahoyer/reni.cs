using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class Statement : DumpableObject
    {
        internal static Result<Statement[]> CreateStatements(Result<Value> value)
            => Create(value).Convert(x => new[] {x});

        internal static Result<Statement> Create(Result<Value> value)
            => value.Convert(x => new Statement(null, null, x));

        internal static Result<Statement> Create
            (IDeclarationTag[] tags, Definable target, Result<Value> body)
            => body.Convert(x => new Statement(tags, target, x));

        Statement
            (IDeclarationTag[] tags, Definable target, Value body)
        {
            Target = target;
            Body = body;
            Tags = tags?? new IDeclarationTag[0];
            StopByObjectIds();
        }

        [EnableDump]
        IDeclarationTag[] Tags { get; }
        [EnableDump]
        Definable Target { get; }
        [EnableDump]
        internal Value Body { get; }

        string Name => Target?.Id;

        [DisableDump]
        internal bool IsMixInSyntax => Tags.Any(item => item is MixInDeclarationToken);

        [DisableDump]
        internal bool IsConverterSyntax => Tags.Any(item => item is ConverterToken);

        [DisableDump]
        internal bool IsMutableSyntax => Tags.Any(item => item is MutableDeclarationToken);

        protected override string GetNodeDump()
            => base.GetNodeDump() +
                (Name == null ? "" : "(" + Name + ")");

        internal IEnumerable<string> GetDeclarations()
        {
            if(Target != null)
                yield return Target.Id;
        }
    }
}