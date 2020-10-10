using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Parser
{
    /// <summary>
    ///     Static syntax items
    /// </summary>
    abstract class Syntax : DumpableObject, ITree<Syntax>, ValueCache.IContainer
    {
        internal abstract class NoChildren : Syntax
        {
            protected NoChildren(BinaryTree target)
                : base(target) { }

            protected NoChildren(int objectId, BinaryTree target)
                : base(objectId, target) { }

            [DisableDump]
            protected sealed override int LeftChildCount => 0;
            [DisableDump]
            protected sealed override int DirectChildCount => 0;

            protected sealed override Syntax GetDirectChild(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChild)}({index})");
        }

        internal readonly BinaryTree Target;

        protected Syntax(BinaryTree target) => Target = target;

        protected Syntax(int objectId, BinaryTree target)
            : base(objectId)
            => Target = target;

        [DisableDump]
        protected abstract int LeftChildCount { get; }
        [DisableDump]
        protected abstract int DirectChildCount { get; }

        internal Syntax[] DirectChildren => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();
        int ITree<Syntax>.DirectChildCount => DirectChildCount;
        Syntax ITree<Syntax>.GetDirectChild(int index) => GetDirectChild(index);

        int ITree<Syntax>.LeftDirectChildCount => LeftChildCount;

        protected abstract Syntax GetDirectChild(int index);

        internal virtual Result<ValueSyntax> ToValueSyntax(BinaryTree target = null)
        {
            if(GetType().Is<ValueSyntax>())
                return (ValueSyntax)this;

            NotImplementedMethod(target);
            return default;
        }

        internal Result<CompoundSyntax> ToCompoundSyntax(BinaryTree target = null)
            => this is CompoundSyntax compoundSyntax
                ? compoundSyntax
                : ToCompoundSyntaxHandler(target);

        protected virtual Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target = null)
        {
            NotImplementedMethod(target);
            return default;
        }

        internal virtual DeclarationSyntax[] ToDeclarationsSyntax(BinaryTree target = null)
        {
            NotImplementedMethod(target);
            return default;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this.GetNodesFromLeftToRight().SelectMany(node => node.CheckedItemsAsLongAs(condition));

    }
}