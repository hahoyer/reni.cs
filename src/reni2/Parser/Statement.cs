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
            => value.Convert(x => new Statement(null, null, null, token, x));

        internal static Result<Statement> Create
            (IDeclarationTag[] tags, Definable target, SourcePart targetToken, SourcePart token, Result<Value> body)
            => body.Convert(x => new Statement(tags, target, targetToken, token, x));

        Statement
            (IDeclarationTag[] tags, Definable target, SourcePart targetToken, SourcePart token, Value body)
        {
            TargetToken = targetToken;
            Target = target;
            Token = token;
            Body = body;
            Tags = tags?? new IDeclarationTag[0];
            StopByObjectIds();
        }

        [DisableDump]
        SourcePart TargetToken { get; }

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

        [DisableDump]
        internal SourcePart SourcePart
        {                          
            get
            {
                if(Name == null && !IsConverterSyntax && !IsMixInSyntax && !IsMutableSyntax)
                    return Body.SourcePart;

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal SourcePosn SourceStart
        {
            get
            {
                if(Name != null)
                    return TargetToken.Start;

                if(!IsConverterSyntax && !IsMixInSyntax && !IsMutableSyntax && Body != null)
                    return Body.SourceStart;

                NotImplementedMethod();
                return null;
            }
        }

        [DisableDump]
        internal SourcePosn SourceEnd
        {
            get
            {
                if (Body != null)
                    return Body.SourceEnd;

                NotImplementedMethod();
                return null;
            }
        }

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