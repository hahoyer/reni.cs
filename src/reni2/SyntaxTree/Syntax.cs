using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Helper;
using Reni.Parser;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    /// <summary>
    ///     Static syntax items
    /// </summary>
    public abstract class Syntax : DumpableObject, ITree<Syntax>, ValueCache.IContainer
    {
        internal abstract class NoChildren : Syntax
        {
            protected NoChildren(Anchor anchor, Issue issue = null)
                : base(anchor, issue) { }

            [DisableDump]
            protected sealed override int LeftDirectChildCountInternal => 0;

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

        internal readonly Anchor Anchor;

        [EnableDumpExcept(null)]
        readonly Issue Issue;

        protected Syntax(Anchor anchor, Issue issue = null)
        {
            Anchor = anchor;
            Anchor.SourceParts.AssertIsNotNull();
            Issue = issue;
        }

        protected Syntax(int objectId, Anchor anchor, Issue issue = null)
            : base(objectId)
        {
            Anchor = anchor;
            Anchor.SourceParts.AssertIsNotNull();
            Issue = issue;
        }

        [DisableDump]
        internal int LeftDirectChildCount => LeftDirectChildCountInternal;

        [DisableDump]
        protected abstract int LeftDirectChildCountInternal { get; }

        [DisableDump]
        protected abstract int DirectChildCount { get; }

        [DisableDump]
        internal IEnumerable<Syntax> Children => this.GetNodesFromLeftToRight();

        internal Syntax[] DirectChildren => this.CachedValue(() => DirectChildCount.Select(GetDirectChild).ToArray());

        [DisableDump]
        internal Issue[] Issues => this.CachedValue(() => GetIssues().ToArray());

        ValueCache ValueCache.IContainer.Cache { get; } = new ValueCache();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ITree<Syntax>.DirectChildCount => DirectChildCount;

        Syntax ITree<Syntax>.GetDirectChild(int index)
            => index < 0 || index >= DirectChildCount? null : DirectChildren[index];

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ITree<Syntax>.LeftDirectChildCount => LeftDirectChildCountInternal;

        IEnumerable<Issue> GetIssues() => T(Issue).Concat(Anchor.Issues).Where(node => node != null);

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

        internal virtual Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target = null)
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
            level ??= new Level();

            foreach(var node in DirectChildren.Where(node => node != this))
                node?.AssertValid();
        }
    }
}