using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.SyntaxTree
{
    /// <summary>
    ///     Static syntax items
    /// </summary>
    abstract class  Syntax : DumpableObject, ITree<Syntax>, ValueCache.IContainer, Feature.ISourceProvider
    {
        internal abstract class NoChildren : Syntax
        {
            protected NoChildren(BinaryTree anchor)
                : base(anchor) { }

            [DisableDump]
            internal sealed override int LeftDirectChildCount => 0;

            [DisableDump]
            protected sealed override int DirectChildCount => 0;

            protected sealed override Syntax GetDirectChild(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChild)}({index})");
        }

        internal readonly BinaryTree Anchor;

        protected Syntax(BinaryTree anchor) => Anchor = anchor;

        protected Syntax(int objectId, BinaryTree anchor)
            : base(objectId)
            => Anchor = anchor;

        [DisableDump]
        internal abstract int LeftDirectChildCount { get; }

        [DisableDump]
        IEnumerable<Syntax> Children => this.GetNodesFromLeftToRight();

        [DisableDump]
        protected abstract int DirectChildCount { get; }

        internal Syntax[] DirectChildren => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ITree<Syntax>.LeftDirectChildCount => LeftDirectChildCount;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ITree<Syntax>.DirectChildCount => DirectChildCount;

        Syntax ITree<Syntax>.GetDirectChild(int index) 
            => index < 0 || index >= DirectChildCount? null : DirectChildren[index];

        protected abstract Syntax GetDirectChild(int index);

        internal virtual Result<ValueSyntax> ToValueSyntax()
        {
            if(GetType().Is<ValueSyntax>())
                return (ValueSyntax)this;

            NotImplementedMethod();
            return default;
        }

        internal Result<CompoundSyntax> ToCompoundSyntax(BinaryTree target = null)
            => ToCompoundSyntaxHandler(target);

        protected virtual Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target = null)
        {
            NotImplementedMethod(target);
            return default;
        }

        internal virtual Result<StatementSyntax[]> ToStatementsSyntax(BinaryTree target = null)
        {
            NotImplementedMethod(target);
            return default;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this.GetNodesFromLeftToRight().SelectMany(node => node.CheckedItemsAsLongAs(condition));


        internal class Level
        {
            public bool IsCorrectOrder;
            public bool IsCorrectMapping;
        }


        internal virtual void AssertValid(Level level = null, BinaryTree target = null)
        {
            level ??= new Level {IsCorrectOrder= true};

            foreach(var node in DirectChildren.Where(node=>node != this))
                node?.AssertValid(null);

            target ??= Anchor;
            if(target == null)
                return;

            var nodesInSyntax = this
                .GetNodesFromLeftToRight()
                .Select(node => node?.Anchor)
                .Where(node => node != null)
                .ToArray();
            
            if(level.IsCorrectOrder)
            for(var index = 1; index < nodesInSyntax.Length; index++)
            {
                var last= nodesInSyntax[index-1].Token.Characters;
                var current= nodesInSyntax[index].Token.Characters;
                Tracer.Assert(last < current);
            }

            if(level.IsCorrectMapping)
            {
                var missingTargetNodes = target.GetNodesFromLeftToRight()
                    .Where(node => !(node?.TokenClass is IRightBracket))
                    .Where(node => !(node?.TokenClass is ILeftBracket))
                    .Where(node => !(node?.TokenClass is List))
                    .Where(node => !(node?.TokenClass is ThenToken))
                    .Where(node => !(node?.TokenClass is ExclamationBoxToken))
                    .Where(node => !nodesInSyntax.Contains(node))
                    .ToArray();

                if(missingTargetNodes.Any())
                {
                    var targetNodes = target.GetNodesFromLeftToRight()
                        .ToArray();

                    NotImplementedMethod(target, nameof(missingTargetNodes), missingTargetNodes);
                }

            }

        }

        SourcePart Feature.ISourceProvider.Value => SourcePart;

        SourcePart SourcePart => Anchor?.SourcePart;
    }
}