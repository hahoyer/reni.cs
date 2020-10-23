using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Helper;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;
using Reni.Validation;

namespace Reni.SyntaxTree
{
    /// <summary>
    ///     Static syntax items that represent a value
    /// </summary>
    abstract class ValueSyntax : Syntax, IStatementSyntax
    {
        internal new abstract class NoChildren : ValueSyntax
        {
            protected NoChildren(BinaryTree anchor, FrameItemContainer frameItems, Issue issue = null)
                : base(anchor, frameItems, issue)
                => anchor.AssertIsNotNull();

            [DisableDump]
            protected sealed override int LeftDirectChildCountKernel => 0;

            [DisableDump]
            protected sealed override int DirectChildCountKernel => 0;

            protected sealed override Syntax GetDirectChildKernel(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChildKernel)}({index})");
        }

        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        protected ValueSyntax(BinaryTree anchor, FrameItemContainer frameItems = null, Issue issue = null)
            : base(anchor, issue, frameItems) { }

        protected ValueSyntax(int objectId, BinaryTree anchor, FrameItemContainer frameItems = null, Issue issue = null)
            : base(objectId, anchor, issue, frameItems) { }


        [DisableDump]
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual bool? IsHollow => IsLambda? (bool?)true : null;

        [DisableDump]
        internal virtual IRecursionHandler RecursionHandler => null;

        [DisableDump]
        internal Issue[] AllIssues => this.CachedValue(GetAllIssues);

        DeclarerSyntax IStatementSyntax.Declarer => null;

        ValueSyntax IStatementSyntax.ToValueSyntax(FrameItemContainer frameItems) => this;

        ValueSyntax IStatementSyntax.Value => this;

        Issue[] GetAllIssues() => this
            .GetNodesFromLeftToRight()
            .SelectMany(node => node?.Issues)
            .ToArray();

        internal override Result<IStatementSyntax[]> ToStatementsSyntax(BinaryTree target = null)
            => T((IStatementSyntax)this);

        //[DebuggerHidden]
        internal virtual Result ResultForCache(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        internal void AddToCacheForDebug(ContextBase context, ResultCache cacheItem)
            => ResultCache.Add(context, cacheItem);

        internal Result Result(ContextBase context) => context.Result(Category.All, this);

        internal BitsConst Evaluate(ContextBase context)
            => Result(context).Evaluate(context.RootContext.ExecutionContext);

        //[DebuggerHidden]
        internal TypeBase Type(ContextBase context) => context.Result(Category.Type, this)?.Type;

        internal bool IsHollowStructureElement(ContextBase context)
        {
            var result = IsHollow;
            if(result != null)
                return result.Value;

            var type = context.TypeIfKnown(this);
            return (type ?? Type(context)).SmartUn<FunctionType>().IsHollow;
        }

        internal ValueSyntax ReplaceArg(ValueSyntax syntax)
            => Visit(new ReplaceArgVisitor(syntax)) ?? this;

        internal virtual ValueSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        internal IEnumerable<string> GetDeclarationOptions(ContextBase context)
            => Type(context).DeclarationOptions;
    }
}