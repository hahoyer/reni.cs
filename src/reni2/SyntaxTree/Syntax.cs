using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    /// <summary>
    ///     Static syntax items
    /// </summary>
    abstract class Syntax : DumpableObject, ITree<Syntax>, ValueCache.IContainer, Feature.ISourceProvider
    {
        internal abstract class NoChildren : Syntax
        {
            protected NoChildren(BinaryTree anchor, Issue issue = null)
                : base(anchor, issue) { }

            [DisableDump]
            internal sealed override int LeftDirectChildCount => 0;

            [DisableDump]
            protected sealed override int DirectChildCount => 0;

            protected sealed override Syntax GetDirectChild(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChild)}({index})");
        }


        internal class Level
        {
            public bool IsCorrectMapping;
            public bool IsCorrectOrder;
        }

        internal readonly BinaryTree Anchor;

        [EnableDumpExcept(null)]
        internal readonly Issue Issue;

        protected Syntax(BinaryTree anchor, Issue issue = null)
        {
            Anchor = anchor;
            Issue = issue;
            Anchor.AssertIsNotNull();
        }

        protected Syntax(int objectId, BinaryTree anchor, Issue issue = null)
            : base(objectId)
        {
            Anchor = anchor;
            Issue = issue;
            Anchor.AssertIsNotNull();
        }

        [DisableDump]
        internal abstract int LeftDirectChildCount { get; }

        [DisableDump]
        IEnumerable<Syntax> Children => this.GetNodesFromLeftToRight();

        [DisableDump]
        protected abstract int DirectChildCount { get; }

        internal Syntax[] DirectChildren => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        internal SourcePart SourcePart => Anchor?.SourcePart;

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        SourcePart Feature.ISourceProvider.Value => SourcePart;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ITree<Syntax>.DirectChildCount => DirectChildCount;

        Syntax ITree<Syntax>.GetDirectChild(int index)
            => index < 0 || index >= DirectChildCount? null : DirectChildren[index];

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ITree<Syntax>.LeftDirectChildCount => LeftDirectChildCount;

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

        internal virtual Result<IStatementSyntax[]> ToStatementsSyntax(BinaryTree target = null)
        {
            NotImplementedMethod(target);
            return default;
        }

        internal IEnumerable<Syntax> ItemsAsLongAs(Func<Syntax, bool> condition)
            => this.GetNodesFromLeftToRight().SelectMany(node => node.CheckedItemsAsLongAs(condition));


        internal virtual void AssertValid(Level level = null, BinaryTree target = null)
        {
            level ??= new Level {IsCorrectOrder = true};

            foreach(var node in DirectChildren.Where(node => node != this))
                node?.AssertValid();

            target ??= Anchor;
            if(target == null)
                return;

            var nodes = this
                .GetNodesFromLeftToRight()
                .ToArray();

            var paths = nodes
                .Select(node => this.GetPaths(child => node == child).ToArray())
                .ToArray();

            var nodesInSyntax = this
                .GetNodesFromLeftToRight()
                .Select(node => node?.Anchor)
                .Where(node => node != null)
                .ToArray();


            if(level.IsCorrectOrder)
                for(var index = 1; index < nodesInSyntax.Length; index++)
                {
                    var last = nodesInSyntax[index - 1].Token.Characters;
                    var current = nodesInSyntax[index].Token.Characters;

                    (last < current).Assert(() =>
                        $"Last: {Tracer.Dump(paths[index - 1])} {nodes[index - 1].Dump()} \n" +
                        $"Current: {Tracer.Dump(paths[index])} {nodes[index].Dump()}");
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
    }
}