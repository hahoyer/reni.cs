using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Parser
{
    sealed class Statement : DumpableObject
    {
        class CacheContainer
        {
            public ValueCache<string[]> AllNames;
            public ValueCache<string[]> PublicNames;
            public ValueCache<Result<Syntax>> Syntax;
        }

        [EnableDump]
        internal Syntax Body { get; }

        readonly CacheContainer Cache = new CacheContainer();

        IDefaultScopeProvider Container { get; }

        [EnableDump]
        IDeclarationTag[] Tags { get; }

        [EnableDump]
        Definable Target { get; }

        Statement(IDeclarationTag[] tags, Definable target, Syntax body, IDefaultScopeProvider container)
        {
            Target = target;
            Body = body;
            Container = container;
            Tags = tags ?? new IDeclarationTag[0];
            Cache.Syntax = new ValueCache<Result<Syntax>>(GetSyntax);
            Cache.AllNames = new ValueCache<string[]>(GetAllNames);
            Cache.PublicNames = new ValueCache<string[]>(GetPublicNames);
            StopByObjectIds();
        }

        [DisableDump]
        internal Syntax Syntax => Cache.Syntax.Value.Target;

        [DisableDump]
        internal Issue[] Issues => Cache.Syntax.Value.Issues;

        [DisableDump]
        internal IEnumerable<string> AllNames => Cache.AllNames.Value;

        [DisableDump]
        internal IEnumerable<string> PublicNames => Cache.PublicNames.Value;

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

        string Name => Target?.Id;

        public Statement Visit(ISyntaxVisitor visitor)
        {
            var newBody = Body.Visit(visitor);
            return newBody == null? this : new Statement(Tags, Target, newBody, Container);
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() +
               (Name == null? "" : "(" + Name + ")");

        internal bool IsDefining(string name, bool publicOnly)
            => (publicOnly? PublicNames : AllNames)
                .Contains(name);

        internal static Result<Statement[]> CreateStatements(Result<Syntax> value, IDefaultScopeProvider container)
            => Create(value, container).Convert(x => new[] {x});

        internal static Result<Statement> Create(Result<Syntax> value, IDefaultScopeProvider container)
            => value.Convert(x => new Statement(null, null, x, container));

        internal static Result<Statement> Create
            (IDeclarationTag[] tags, Definable target, Result<Syntax> body, IDefaultScopeProvider container)
            => body.Convert(x => new Statement(tags, target, x, container));

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

        internal Syntax GetChildren() => Body;

        Result<Syntax> GetSyntax() => Body;
        string[] GetAllNames() => GetAllDeclarations().ToArray();
        string[] GetPublicNames() => GetPublicDeclarations().ToArray();
    }

    interface IDefaultScopeProvider
    {
        bool MeansPublic { get; }
    }
}