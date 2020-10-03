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
    sealed class Value : ValueWithParent<Value>
    {
        class CacheContainer
        {
            public FunctionCache<int, Value> ExtendedLocateByPosition;
            public FunctionCache<int, Value> LocateByPosition;
        }

        readonly CacheContainer Cache = new CacheContainer();

        internal Value(Reni.Parser.Value target)
            : this(target, null) { }

        Value(Reni.Parser.Value target, Value parent)
            : base(target, parent)
        {
            Cache.LocateByPosition = new FunctionCache<int, Value>(LocateByPositionForCache);
            Cache.ExtendedLocateByPosition = new FunctionCache<int, Value>(ExtendedLocateByPositionForCache);
        }

        protected override string GetNodeDump() => $"{GetType().PrettyName()}({SourcePart.NodeDump})";

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
        internal IEnumerable<Value> ParentChainIncludingThis
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
        IEnumerable<Value> Items => this.CachedValue(GetItems);

        [DisableDump]
        bool IsFunctionLevel => TokenClass is Function;

        public Value LocateByPosition(int current) => Cache.LocateByPosition[current];

        protected override Value Create(Reni.Parser.Value target, Value parent) => new Value(target, parent);

        internal Value Locate(SourcePart part)
        {
            NotImplementedMethod(part);
            return default;
        }


        IEnumerable<Value> GetItems()
        {
            var left = LeftChildren
                .SelectMany(item => item?.Items ?? new Value[0]);

            var right = RightChildren
                .SelectMany(item => item?.Items ?? new Value[0]);

            return T(left, T(this), right).Concat();
        }

        Value CheckedLocate(SourcePart part)
            => SourcePart.Contains(part)? Locate(part) : null;

        Value LocatePositionExtended(int current) => Cache.ExtendedLocateByPosition[current];

        Value LocateByPositionForCache(int current)
        {
            var s = SourcePart;
            var i = Items.ToArray();
            NotImplementedMethod(current);
            return default;
        }

        Value ExtendedLocateByPositionForCache(int current)
        {
            NotImplementedMethod(current);
            return default;
        }


        Value LocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocateByPosition(current)
                    : null;

        Value ExtendedLocateByPositionOrDefault(int current)
            =>
                Contains(current)
                    ? LocatePositionExtended(current)
                    : null;
    }
}