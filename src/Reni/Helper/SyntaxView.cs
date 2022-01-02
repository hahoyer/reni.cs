using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.SyntaxTree;

namespace Reni.Helper
{
    public abstract class SyntaxView<TTarget> : DumpableObject, ValueCache.IContainer, ITree<TTarget>
        where TTarget : SyntaxView<TTarget>
    {
        sealed class CacheContainer
        {
            public FunctionCache<int, TTarget> DirectChildren;
            public FunctionCache<SourcePosition, (TTarget, int)> LocateByPosition;
            public FunctionCache<SourcePosition, (TTarget, int)> LocateByPositionIncludingParent;
        }

        internal readonly PositionDictionary<TTarget> Context;

        internal readonly Syntax FlatItem;
        internal readonly TTarget Parent;
        readonly CacheContainer Cache = new();

        protected SyntaxView(Syntax flatItem, TTarget parent, PositionDictionary<TTarget> context)
        {
            flatItem.AssertIsNotNull();
            FlatItem = flatItem;
            Parent = parent;
            Context = context;

            foreach(var anchor in FlatItem.Anchor.Items)
                Context[anchor] = (TTarget)this;

            Cache.LocateByPosition =
                new(i => LocateByPositionForCache(i, false));
            Cache.LocateByPositionIncludingParent
                = new(i => LocateByPositionForCache(i, true));
            Tracer.ConditionalBreak(flatItem.ObjectId == -492);
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();
        int ITree<TTarget>.DirectChildCount => DirectChildCount;
        TTarget ITree<TTarget>.GetDirectChild(int index) => DirectChildren[index];

        int ITree<TTarget>.LeftDirectChildCount => 0;

        protected abstract TTarget Create(Syntax syntax, int index);

        [DisableDump]
        public SourcePart[] Anchors => FlatItem.Anchor.SourceParts;

        int DirectChildCount => FlatItem.DirectChildren.Length;

        internal TTarget[] DirectChildren
            => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        TTarget GetDirectChild(int index)
        {
            var child = FlatItem.DirectChildren[index];
            return child == null? null : Create(child, index);
        }

        (TTarget Master, int Index) LocateByPositionForCache(SourcePosition current, bool includingParent)
        {
            (!includingParent).Assert();
            return Context[current]
                .LocateByPosition(current);
        }

        (TTarget Master, int Index) LocateByPosition(SourcePosition current)
        {
            var result = Anchors
                .Select((anchor, index) => (anchor, index))
                .FirstOrDefault(node => node.anchor.Length > 0 && node.anchor.Contains(current));

            if(result.anchor != null)
                return ((TTarget)this, result.index);

            NotImplementedMethod(current);
            return default;
        }

        internal(TTarget Master, int Index) LocateByPosition(SourcePosition offset, bool includingParent)
            => includingParent? Cache.LocateByPositionIncludingParent[offset] : Cache.LocateByPosition[offset];
    }
}