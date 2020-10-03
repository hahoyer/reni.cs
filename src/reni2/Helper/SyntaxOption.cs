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
        internal const bool ParentIsObsolete = false;

        class CacheContainer
        {
            public SyntaxOption Parent;
        }

        readonly CacheContainer Cache = new CacheContainer();


        [DisableDump]
        readonly BinaryTree Target;

        public SyntaxOption(BinaryTree target)
        {
            Target = target;
            SetParent();
        }

        [Obsolete("", ParentIsObsolete)]
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
        internal IEnumerable<BinaryTree> Items => this.CachedValue(GetItems);

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

        internal SourcePart SourcePart =>
            LeftMost.Target.Token.SourcePart().Start.Span(RightMost.Target.Token.Characters.End);

        [DisableDump]
        SyntaxOption LeftMost => Target.Left?.Option.LeftMost ?? this;

        [DisableDump]
        SyntaxOption RightMost => Target.Right?.Option.RightMost ?? this;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        [Obsolete("", ParentIsObsolete)]
        void SetParent()
        {
            if(Target.Left != null)
                Target.Left.Option.Parent = this;

            if(Target.Right != null)
                Target.Right.Option.Parent = this;
        }

        IEnumerable<BinaryTree> GetItems()
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