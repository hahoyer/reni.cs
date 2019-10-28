using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Code;
using Reni.TokenClasses;

namespace Reni.Parser
{
    sealed class Statement : DumpableObject
    {
        internal static Result<Statement[]> CreateStatements(Result<Value> value, IDefaultScopeProvider container)
            => Create(value, container).Convert(x => new[] {x});

        internal static Result<Statement> Create(Result<Value> value, IDefaultScopeProvider container)
            => value.Convert(x => new Statement(null, null, x,container));

        internal static Result<Statement> Create
            (IDeclarationTag[] tags, Definable target, Result<Value> body, IDefaultScopeProvider container)
            => body.Convert(x => new Statement(tags, target, x,container));

        Statement(IDeclarationTag[] tags, Definable target, Value body, IDefaultScopeProvider container)
        {
            Target = target;
            Body = body;
            Container = container;
            Tags = tags ?? new IDeclarationTag[0];
            StopByObjectIds();
        }

        public Statement Visit(ISyntaxVisitor visitor)
        {
            var newBody = Body.Visit(visitor);
            return newBody == null ? this : new Statement(Tags, Target, newBody, Container);
        }

        [EnableDump]
        IDeclarationTag[] Tags { get; }
        [EnableDump]
        Definable Target { get; }
        [EnableDump]
        internal Value Body { get; }

        IDefaultScopeProvider Container { get; }

        string Name => Target?.Id;

        [DisableDump]
        internal bool IsMixInSyntax => Tags.Any(item => item is MixInDeclarationToken);

        [DisableDump]
        internal bool IsConverterSyntax => Tags.Any(item => item is ConverterToken);

        [DisableDump]
        internal bool IsMutableSyntax => Tags.Any(item => item is MutableDeclarationToken);

        [DisableDump]
        internal bool IsPublicSyntax
        {
            get
            {
                if(Tags.Any(item => item is PublicDeclarationToken))
                    return true;

                if(Tags.Any(item => item is NonPublicDeclarationToken))
                    return false;

                return Container.MeansPublic;
            }
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() +
            (Name == null ? "" : "(" + Name + ")");

        internal IEnumerable<string> GetAllDeclarations()
        {
            if(Target != null)
                yield return Target.Id;
        }

        internal IEnumerable<string> GetPublicDeclarations()
        {
            if(Target != null && IsPublicSyntax)
                yield return Target.Id;
        }

    }

    interface IDefaultScopeProvider
    {
        bool MeansPublic { get; }
    }
}