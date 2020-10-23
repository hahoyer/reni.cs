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
            protected NoChildren(BinaryTree anchor, Issue issue = null, FrameItemContainer frameItems = null)
                : base(anchor, issue, frameItems??FrameItemContainer.Create()) { }

            [DisableDump]
            protected sealed override int LeftDirectChildCountKernel => 0;

            [DisableDump]
            protected sealed override int DirectChildCountKernel => 0;

            protected sealed override Syntax GetDirectChildKernel(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChild)}({index})");
        }


        internal class Level
        {
            public bool IsCorrectMapping;
            public bool IsCorrectOrder;
        }

        internal readonly BinaryTree Anchor;
        internal readonly FrameItemContainer FrameItems;

        [EnableDumpExcept(null)]
        readonly Issue Issue;

        protected Syntax(BinaryTree anchor, Issue issue = null, FrameItemContainer frameItems = null)
        {
            FrameItems = frameItems?? FrameItemContainer.Create();
            Anchor = anchor;
            Issue = issue;
            Anchor.AssertIsNotNull();
        }

        protected Syntax(int objectId, BinaryTree anchor, Issue issue = null, FrameItemContainer frameItems = null)
            : base(objectId)
        {
            FrameItems = frameItems?? FrameItemContainer.Create();
            Anchor = anchor;
            Issue = issue;
            Anchor.AssertIsNotNull();
        }

        [DisableDump]
        protected abstract int LeftDirectChildCountKernel { get; }
        protected abstract int DirectChildCountKernel { get; }

        [DisableDump]
        internal int LeftDirectChildCount => LeftDirectChildCountKernel + FrameItems.LeftItemCount;
        protected int DirectChildCount => DirectChildCountKernel + FrameItems.Items.Length;


        [DisableDump]
        IEnumerable<Syntax> Children => this.GetNodesFromLeftToRight();

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

        [DisableDump]
        internal Issue[] Issues => this.CachedValue(()=>GetIssues().ToArray());

        IEnumerable<Issue> GetIssues()
        {
            if(Issue != null)
                yield return Issue;
            foreach(var issue in Anchor.Issues)
                yield return issue;
        }

        protected abstract Syntax GetDirectChildKernel(int index);

        Syntax GetDirectChild(int index)
            => index < FrameItems.LeftItemCount
                ? FrameItems.Items[index]
                : index < FrameItems.LeftItemCount + DirectChildCountKernel
                    ? GetDirectChildKernel(index - FrameItems.LeftItemCount)
                    : index < FrameItems.Items.Length + DirectChildCountKernel
                        ? FrameItems.Items[index - DirectChildCountKernel]
                        : null;


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
            level ??= new Level {};

            foreach(var node in DirectChildren.Where(node => node != this))
                node?.AssertValid();

            target ??= Anchor;
            if(target == null)
                return;

            var nodes = this
                .GetNodesFromLeftToRight()
                .ToArray();

            var paths = nodes
                .Select(node => this.GetPaths(child => node == child).Stringify("."))
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