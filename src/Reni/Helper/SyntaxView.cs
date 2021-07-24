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
        class CacheContainer
        {
            public FunctionCache<int, TTarget> DirectChildren;
            public FunctionCache<SourcePosition, (TTarget, int)> LocateByPosition;
            public FunctionCache<SourcePosition, (TTarget, int)> LocateByPositionIncludingParent;
        }

        internal readonly PositionDictionary<TTarget> Context;

        internal readonly Syntax FlatItem;
        internal readonly TTarget Parent;
        readonly CacheContainer Cache = new CacheContainer();

        protected SyntaxView(Syntax flatItem, TTarget parent, PositionDictionary<TTarget> context)
        {
            flatItem.AssertIsNotNull();
            FlatItem = flatItem;
            Parent = parent;
            Context = context;

            foreach(var anchor in FlatItem.Anchor.Items)
                Context[anchor] = (TTarget)this;

            Cache.LocateByPosition =
                new FunctionCache<SourcePosition, (TTarget, int)>(i => LocateByPositionForCache(i, false));
            Cache.LocateByPositionIncludingParent
                = new FunctionCache<SourcePosition, (TTarget, int)>(i => LocateByPositionForCache(i, true));
            Tracer.ConditionalBreak(flatItem.ObjectId == -492);
        }

        [DisableDump]
        public SourcePart[] Anchors => FlatItem.Anchor.SourceParts;

        int DirectChildCount => FlatItem.DirectChildren.Length;

        internal TTarget[] DirectChildren
            => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        int ITree<TTarget>.DirectChildCount => DirectChildCount;
        TTarget ITree<TTarget>.GetDirectChild(int index) => DirectChildren[index];

        int ITree<TTarget>.LeftDirectChildCount => 0;

        TTarget GetDirectChild(int index)
        {
            var child = FlatItem.DirectChildren[index];
            return child == null? null : Create(child, index);
        }

        protected abstract TTarget Create(Syntax syntax, int index);

        (TTarget Master, int Index) LocateByPositionForCache(SourcePosition current, bool includingParent)
        {
            (!includingParent).Assert();
            return Context[current]
                .Node
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