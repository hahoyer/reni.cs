using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Parser;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using ReniUI.Formatting;

namespace ReniUI.Helper
{
    sealed class Syntax : SyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, Syntax> LocatePosition;
        }

        readonly CacheContainer Cache = new CacheContainer();


        public Syntax(Reni.TokenClasses.Syntax target, Syntax parent = null)
            : base(target, parent)
            => Cache.LocatePosition = new FunctionCache<int, Syntax>(LocatePositionForCache);

        protected override Syntax Create(Reni.TokenClasses.Syntax target, Syntax parent) => new Syntax(target, parent);

        internal bool LeftSideSeparator
        {
            get
            {
                var left = LeftNeighbor?.TokenClass;
                return !LeftWhiteSpaces.HasComment() && SeparatorExtension.Get(left, Target.TokenClass);
            }
        }

        internal bool RightSideSeparator
        {
            get
            {
                var tokenClass = RightNeighbor?.TokenClass;
                return tokenClass != null &&
                       !RightWhiteSpaces.HasComment() &&
                       SeparatorExtension.Get(Target.TokenClass, tokenClass);
            }
        }

        [DisableDump]
        internal SourcePart SourcePart => Target.SourcePart;

        [DisableDump]
        internal IEnumerable<Syntax> ParentChainIncludingThis
        {
            get
            {
                yield return this;

                if(Parent == null)
                    yield break;

                foreach(var other in Parent.ParentChainIncludingThis)
                    yield return other;
            }
        }

        [DisableDump]
        internal IEnumerable<Syntax> Items => this.CachedValue(GetItems);

        internal Result<Value> Value => Target.Value;
        internal IToken Token => Target.Token;

        IEnumerable<Syntax> GetItems()
        {
            if(Left != null)
                foreach(var sourceSyntax in Left.Items)
                    yield return sourceSyntax;

            yield return this;

            if(Right != null)
                foreach(var sourceSyntax in Right.Items)
                    yield return sourceSyntax;
        }

        internal Syntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        Syntax CheckedLocate(SourcePart part)
            => Target.SourcePart.Contains(part) ? Locate(part) : null;

        public Syntax LocatePosition(int current) => Cache.LocatePosition[current];

        Syntax LocatePositionForCache(int current)
            => Contains(current)
                ? Left?.CheckedLocatePosition(current) ??
                  Right?.CheckedLocatePosition(current) ??
                  this
                : Parent.LocatePositionForCache(current);

        Syntax CheckedLocatePosition(int current)
            =>
                Contains(current)
                    ? LocatePosition(current)
                    : null;

        bool Contains(int current)
            => Target.SourcePart.Position <= current && current < Target.SourcePart.EndPosition;
    }
}