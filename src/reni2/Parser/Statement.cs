using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Scanner;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class Statement : DumpableObject
    {
        internal static Result<Statement[]> CreateStatements(SourcePart token, Result<Value> value)
            => Create(token, value).Convert(x => new[] {x});

        internal static Result<Statement> Create(SourcePart token, Result<Value> value)
            => value.Convert(x => new Statement(null, null, token, x));

        internal static Result<Statement> Create
            (IDeclarationTag[] tags, Definable target, SourcePart token, Result<Value> body)
            => body.Convert(x => new Statement(tags, target, token, x));

        Statement
            (IDeclarationTag[] tags, Definable target, SourcePart token, Value body)
        {
            Target = target;
            Token = token;
            Body = body;
            Tags = tags;
            StopByObjectIds();
        }

        [EnableDump]
        IDeclarationTag[] Tags { get; }
        [EnableDump]
        Definable Target { get; }
        [DisableDump]
        SourcePart Token { get; }
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