using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Struct;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class SyntaxOption : DumpableObject, ValueCache.IContainer
    {
        [DisableDump]
        internal IDefaultScopeProvider DefaultScopeProvider => Target.Option.DefaultScopeProvider;

        class CacheContainer
        {
            public SyntaxOption Parent;
            public FunctionCache<int, Syntax> LocatePosition;
        }

        ValueCache ValueCache.IContainer.Cache {get;} = new ValueCache();

        readonly CacheContainer Cache = new CacheContainer();

        [DisableDump]
        readonly Syntax Target;

        public SyntaxOption Parent
        {
            get => Cache.Parent;
            private set
            {
                Tracer.Assert(value == Cache.Parent || Cache.Parent == null);
                Cache.Parent = value;
            }
        }

        [DisableDump]
        internal SourcePart MainToken => Target.Token.Characters;

        public SyntaxOption(Syntax target)
        {
            Target = target;

            if(Target.Left != null)
                Target.Left.Option.Parent = this;

            if(Target.Right != null)
                Target.Right.Option.Parent = this;

            Cache.LocatePosition = new FunctionCache<int, Syntax>(LocatePositionForCache);
        }


        [DisableDump]
        internal IEnumerable<Syntax> Items => this.CachedValue(GetItems);

        IEnumerable<Syntax> GetItems()
        {
            if(Target.Left != null)
                foreach(var sourceSyntax in Target.Left.Option.Items)
                    yield return sourceSyntax;

            yield return Target;

            if(Target.Right != null)
                foreach(var sourceSyntax in Target.Right.Option.Items)
                    yield return sourceSyntax;
        }

        [DisableDump]
        internal SourcePart SourcePart
            => Target.Left?.Option.SourcePart + Target.Token.Characters + Target.Right?.Option.SourcePart;

        [DisableDump]
        public IEnumerable<Syntax> ParentChainIncludingThis
        {
            get
            {
                yield return Target;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        public Syntax LocatePosition(int current) => Cache.LocatePosition[current];

        Syntax LocatePositionForCache(int current)
        {
            if(current < Target.Option.SourcePart.Position || current >= Target.Option.SourcePart.EndPosition)
                return Parent?.LocatePosition(current);

            return Target.Left?.Option.CheckedLocatePosition(current) ??
                   Target.Right?.Option.CheckedLocatePosition(current) ??
                   Target;
        }

        Syntax CheckedLocatePosition(int current)
            =>
                Target.Option.SourcePart.Position <= current && current < Target.Option.SourcePart.EndPosition
                    ? LocatePosition(current)
                    : null;

        [EnableDumpExcept(null)]
        internal Result<Parser.Value> Value
        {
            get
            {
                if(Target.TokenClass is IDeclarationItem declarationItem && declarationItem.IsDeclarationPart(Target))
                    return null;

                return (Target.TokenClass as IValueProvider)?.Get
                    (Target);
            }
        }

        internal Result<Statement[]> GetStatements(List type = null)
            => (Target.TokenClass as IStatementsProvider)?.Get(type, Target, DefaultScopeProvider);

        [EnableDumpExcept(null)]
        internal Result<Statement[]> Statements => GetStatements();

        [EnableDumpExcept(null)]
        internal Result<Statement> Statement
            =>
                (Target.TokenClass as IStatementProvider)?.Get
                    (Target.Left, Target.Right, DefaultScopeProvider);

        [EnableDumpExcept(null)]
        internal Result<Declarator> Declarator
        {
            get
            {
                var declaratorTokenClass = Target.TokenClass as IDeclaratorTokenClass;
                return declaratorTokenClass?.Get(Target);
            }
        }

        [EnableDumpExcept(null)]
        internal IDeclarationTag DeclarationTag => Target.TokenClass as IDeclarationTag;

        [EnableDumpExcept(null)]
        internal Issue[] Issues
            => Value?.Issues ?? GetStatements()?.Issues ?? Statement?.Issues ?? Declarator?.Issues ?? new Issue[0];

        [DisableDump]
        ContextBase[] Contexts
        {
            get
            {
                if(IsStatementsLevel)
                    return ((CompoundSyntax) Target.Value.Target)
                        .ResultCache
                        .Values
                        .Select(item => item.Type.ToContext)
                        .ToArray();

                var parentContexts = Target.Option.Parent.Contexts;

                if(IsFunctionLevel)
                    return parentContexts
                        .SelectMany(context => FunctionContexts(context, Value.Target))
                        .ToArray();

                return parentContexts;
            }
        }

        static IEnumerable<ContextBase> FunctionContexts(ContextBase context, Parser.Value body)
            => ((FunctionBodyType) context.ResultCache(body).Type)
                .Functions
                .Select(item => item.CreateSubContext(false));

        [DisableDump]
        bool IsFunctionLevel => Target.TokenClass is Function;

        [DisableDump]
        bool IsStatementsLevel
        {
            get
            {
                if(Target.TokenClass is EndOfText)
                    return true;

                if(Value != null)
                    return false;
                if(Statements != null)
                    return Target.Option.Parent?.Target.TokenClass != Target.TokenClass;
                if(Statement != null)
                    return false;

                if(Target.TokenClass is LeftParenthesis)
                    return false;

                if(Target.TokenClass is BeginOfText)
                    return false;

                NotImplementedMethod();
                return false;
            }
        }

        [DisableDump]
        internal IEnumerable<string> Declarations
        {
            get
            {
                if(Value != null)
                    return Contexts
                        .SelectMany(item => Value.Target.DeclarationOptions(item));

                if(Statements != null || Statement != null)
                    return Contexts
                        .SelectMany(item => item.DeclarationOptions)
                        .Concat(DeclarationTagToken.DeclarationOptions);

                if(Declarator != null ||
                   Target.TokenClass is LeftParenthesis ||
                   Target.TokenClass is DeclarationTagToken)
                    return new string[0];

                NotImplementedMethod();
                return null;
            }
        }


        public bool IsDeclarationPart()
        {
            var parentTokenClass = Parent.Target.TokenClass;
            if(parentTokenClass is Colon)
                return Parent.Target.Left == Target;

            if(parentTokenClass is LeftParenthesis ||
               parentTokenClass is Definable ||
               parentTokenClass is ThenToken ||
               parentTokenClass is List ||
               parentTokenClass is Function ||
               parentTokenClass is TypeOperator ||
               parentTokenClass is ElseToken ||
               parentTokenClass is ScannerSyntaxError)
                return false;

            Tracer.FlaggedLine(nameof(Target) + "=" + Target);
            return false;
        }
    }
}