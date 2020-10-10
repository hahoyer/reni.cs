using System;
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

            protected sealed override int LeftChildCount => 0;
            protected sealed override int DirectChildCount => 0;
            protected sealed override Syntax GetDirectChild(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChild)}({index})");
        }

        internal readonly BinaryTree Target;

        protected Syntax(BinaryTree target) => Target = target;

        protected Syntax(int objectId, BinaryTree target)
            : base(objectId)
            => Target = target;

        protected abstract int LeftChildCount { get; }
        protected abstract int DirectChildCount { get; }

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        int ITree<Syntax>.LeftDirectChildCount => LeftChildCount;
        int ITree<Syntax>.DirectChildCount => DirectChildCount;
        Syntax ITree<Syntax>.GetDirectChild(int index) => GetDirectChild(index);
        
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

    }
}