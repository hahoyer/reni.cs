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
            public ValueCache<Result<ValueSyntax>> Syntax;
        }

        [EnableDump]
        internal ValueSyntax Body { get; }

        readonly CacheContainer Cache = new CacheContainer();

        IDefaultScopeProvider Container { get; }

        [EnableDump]
        readonly Declarer Declarer;


        Statement(Declarer declarer, ValueSyntax body, IDefaultScopeProvider container)
        {
            Body = body;
            Container = container;
            Declarer = declarer?? new Declarer(null,null,null);
            Tracer.Assert(Tags != null);
            Cache.Syntax = new ValueCache<Result<ValueSyntax>>(GetSyntax);
            Cache.AllNames = new ValueCache<string[]>(GetAllNames);
            Cache.PublicNames = new ValueCache<string[]>(GetPublicNames);
            StopByObjectIds();
        }

        IDeclarationTag[] Tags => Declarer.Tags;
        Definable Target => Declarer.Target;

        [DisableDump]
        internal ValueSyntax Syntax => Cache.Syntax.Value.Target;

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
            return newBody == null? this : new Statement(Declarer, newBody, Container);
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() +
               (Name == null? "" : "(" + Name + ")");

        internal bool IsDefining(string name, bool publicOnly)
            => (publicOnly? PublicNames : AllNames)
                .Contains(name);

        internal static Result<Statement[]> CreateStatements(Result<ValueSyntax> value, IDefaultScopeProvider container)
            => Create(value, container).Convert(x => new[] {x});

        internal static Result<Statement> Create(Result<ValueSyntax> value, IDefaultScopeProvider container)
            => value.Convert(x => new Statement(null, x, container));

        internal static Result<Statement> Create(Declarer declarer, Result<ValueSyntax> body, IDefaultScopeProvider container)
            => body.Convert(x => new Statement(declarer, x, container));

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

        internal ValueSyntax GetChildren() => Body;

        Result<ValueSyntax> GetSyntax() => Body;
        string[] GetAllNames() => GetAllDeclarations().ToArray();
        string[] GetPublicNames() => GetPublicDeclarations().ToArray();
    }

    interface IDefaultScopeProvider
    {
        bool MeansPublic { get; }
    }
}