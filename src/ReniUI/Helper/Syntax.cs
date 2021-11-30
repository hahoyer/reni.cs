using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;

namespace ReniUI.Helper
{
    public sealed class Syntax : DumpableObject, ValueCache.IContainer, ITree<Syntax>
    {
        internal readonly Reni.SyntaxTree.Syntax FlatItem;
        [DisableDump]
        readonly Syntax Parent;
        [DisableDump]
        readonly PositionDictionary<Syntax> Context;

        internal Syntax(Reni.SyntaxTree.Syntax flatItem, PositionDictionary<Syntax> context)
            : this(flatItem, context, null) { }


        Syntax(Reni.SyntaxTree.Syntax flatItem, PositionDictionary<Syntax> context, Syntax parent)
        {
            FlatItem = flatItem;
            Context = context;
            Parent = parent;

            foreach(var anchor in FlatItem.Anchor.Items)
                Context[anchor] = this;
        }

        ValueCache ValueCache.IContainer.Cache { get; } = new();

        int ITree<Syntax>.DirectChildCount => DirectChildCount;

        Syntax ITree<Syntax>.GetDirectChild(int index) => DirectChildren[index];
        int ITree<Syntax>.LeftDirectChildCount => 0;

        int DirectChildCount => FlatItem.DirectChildren.Length;

        [DisableDump]
        Syntax[] DirectChildren => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        Syntax GetDirectChild(int index)
        {
            var child = FlatItem.DirectChildren[index];
            return child == null? null : Create(child, index);
        }

        Syntax Create(Reni.SyntaxTree.Syntax flatItem, int index) => new(flatItem, Context, this);
    }
}