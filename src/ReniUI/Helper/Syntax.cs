using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.TokenClasses;
using Reni.Validation;

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

        internal Syntax(Reni.Parser.Syntax target)
            : this(target, null) { }

        Syntax(Reni.Parser.Syntax target, Syntax parent)
            : base(target, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, Syntax>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, Syntax>(ExtendedLocateByPositionForCache);
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Issue[] Issues
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public BinaryTreeSyntax BinaryTreeSyntax
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
        }

        [DisableDump]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string[] DeclarationOptions
        {
            get
            {
                NotImplementedMethod();
                return default;
            }
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

        [DisableDump]
        IEnumerable<Syntax> Items => this.CachedValue(GetItems);

        [DisableDump]
        bool IsFunctionLevel => TokenClass is Function;

        protected override string GetNodeDump() => $"{GetType().PrettyName()}({SourcePart.NodeDump})";

        public Syntax LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override Syntax Create(Reni.Parser.Syntax target, Syntax parent) => new Syntax(target, parent);

        internal Syntax Locate(SourcePart part)
        {
            NotImplementedMethod(part);
            return default;
        }


        IEnumerable<Syntax> GetItems()
        {
            var left = LeftChildren
                .SelectMany(item => item?.Items ?? new Syntax[0]);

            var right = RightChildren
                .SelectMany(item => item?.Items ?? new Syntax[0]);

            return T(left, T(this), right).Concat();
        }

        Syntax CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        Syntax LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        Syntax LocateByPositionForCache(int current)
        {
            var s = SourcePart;
            var i = Items.ToArray();
            NotImplementedMethod(current);
            return default;
        }

        Syntax ExtendedLocateByPositionForCache(int current)
        {
            NotImplementedMethod(current);
            return default;
        }


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