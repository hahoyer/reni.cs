using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Helper
{
    sealed class SyntaxOption
        : DumpableObject
            , ValueCache.IContainer
    {
        const bool UseParent = false;

        class CacheContainer
        {
            public SyntaxOption Parent;
        }

        readonly CacheContainer Cache = new CacheContainer();


        [DisableDump]
        readonly Syntax Target;

        public SyntaxOption(Syntax target)
        {
            Target = target;
            SetParent();
        }

        [DisableDump]
        internal IDefaultScopeProvider DefaultScopeProvider => Target.DefaultScopeProvider;

        [Obsolete("", UseParent)]
        internal SyntaxOption Parent
        {
            get => Cache.Parent;
            private set
            {
                Tracer.Assert(value == Cache.Parent || Cache.Parent == null);
                Cache.Parent = value;
            }
        }


        [DisableDump]
        internal IEnumerable<Syntax> Items => this.CachedValue(GetItems);

        [EnableDumpExcept(null)]
        internal Result<Value> Value
        {
            get
            {
                if(Target.TokenClass is IDeclarationItem declarationItem && declarationItem.IsDeclarationPart(Target))
                    return null;

                return (Target.TokenClass as IValueProvider)?.Get
                    (Target);
            }
        }

        [EnableDumpExcept(null)]
        internal Result<Statement[]> Statements => GetStatements();

        [EnableDumpExcept(null)]
        internal Result<Statement> Statement
            =>
                (Target.TokenClass as IStatementProvider)?.Get
                    (Target.Left, Target.Right, DefaultScopeProvider);

        [EnableDumpExcept(null)]
        internal Result<Declarer> Declarer
        {
            get
            {
                var declarerTokenClass = Target.TokenClass as IDeclarerTokenClass;
                return declarerTokenClass?.Get(Target);
            }
        }

        [EnableDumpExcept(null)]
        internal IDeclarationTag DeclarationTag => Target.TokenClass as IDeclarationTag;

        [EnableDumpExcept(null)]
        internal Issue[] Issues
            => Value?.Issues ?? GetStatements()?.Issues ?? Statement?.Issues ?? Declarer?.Issues ?? new Issue[0];

        internal SourcePart SourcePart =>
            LeftMost.Target.Token.SourcePart().Start.Span(RightMost.Target.Token.Characters.End);

        [DisableDump]
        SyntaxOption LeftMost => Target.Left?.Option.LeftMost ?? this;

        [DisableDump]
        SyntaxOption RightMost => Target.Right?.Option.RightMost ?? this;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        internal Result<Statement[]> GetStatements(List type = null)
            => (Target.TokenClass as IStatementsProvider)?.Get(type, Target, DefaultScopeProvider);

        [Obsolete("", UseParent)]
        void SetParent()
        {
            if(Target.Left != null)
                Target.Left.Option.Parent = this;

            if(Target.Right != null)
                Target.Right.Option.Parent = this;
        }

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
    }
}