using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;

namespace ReniUI.Helper
{
    sealed class Syntax : SyntaxWithParent<Syntax>
    {
        class CacheContainer
        {
            public FunctionCache<int, Syntax> ExtendedLocateByPosition;
            public FunctionCache<int, Syntax> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal Syntax(Reni.TokenClasses.BinaryTree target)
            : this(target, null)
        {
        }

        Syntax(Reni.TokenClasses.BinaryTree target, Syntax parent)
            : base(target, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, Syntax>(ExtendedLocateByPositionForCache);
        }

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

        public Syntax LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override Syntax Create(Reni.TokenClasses.BinaryTree target, Syntax parent)
            => new Syntax(target, parent);

        internal Syntax Locate(SourcePart part)
            => Left?.CheckedLocate(part) ??
               Right?.CheckedLocate(part) ??
               this;

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        Syntax LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        Syntax LocateByPositionForCache(int current)
            => Left?.LocateByPositionOrDefault(current) ??
               Right?.LocateByPositionOrDefault(current) ??
               this;

        Syntax ExtendedLocateByPositionForCache(int current)
            => Contains(current)
                ? Left?.ExtendedLocateByPositionOrDefault(current) ??
                  Right?.ExtendedLocateByPositionOrDefault(current) ??
                  this
                : Parent.ExtendedLocateByPositionForCache(current);

        Syntax LocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocateByPosition(current)
                    : null;

        Syntax ExtendedLocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;
    }
}