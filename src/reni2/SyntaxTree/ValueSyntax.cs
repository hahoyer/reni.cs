using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Struct;
using Reni.SyntaxFactory;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.SyntaxTree
{
    /// <summary>
    ///     Static syntax items that represent a value
    /// </summary>
    abstract class ValueSyntax : Syntax, IStatementSyntax
    {
        internal new abstract class NoChildren : ValueSyntax
        {
            protected NoChildren(BinaryTree anchor)
                : base(null, anchor, null) { }

            protected NoChildren(BinaryTree leftBracket, BinaryTree anchor, BinaryTree rightBracket)
                : base(leftBracket, anchor, rightBracket) { }

            [DisableDump]
            protected sealed override int LeftDirectChildCountKernel => 0;

            [DisableDump]
            protected sealed override int DirectChildCountKernel => 0;

            protected sealed override Syntax GetDirectChildKernel(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChildKernel)}({index})");
        }

        class BracketDummy : Syntax.NoChildren
        {
            BracketDummy(BinaryTree anchor)
                : base(anchor) { }

            internal static NoChildren Create(BinaryTree anchor) => anchor == null? null : new BracketDummy(anchor);
        }

        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        [EnableDump]
        [EnableDumpExcept(null)]
        readonly Syntax.NoChildren LeftBracket;
        [EnableDump]
        [EnableDumpExcept(null)]
        readonly Syntax.NoChildren RightBracket;


        protected ValueSyntax(BinaryTree leftBracket, BinaryTree anchor, BinaryTree rightBracket)
            : base(anchor)
        {
            LeftBracket = BracketDummy.Create(leftBracket);
            RightBracket = BracketDummy.Create(rightBracket);
        }

        protected ValueSyntax(int objectId, BinaryTree leftBracket, BinaryTree anchor, BinaryTree rightBracket)
            : base(objectId, anchor)
        {
            LeftBracket = BracketDummy.Create(leftBracket);
            RightBracket = BracketDummy.Create(rightBracket);
        }

        protected abstract int LeftDirectChildCountKernel { get; }
        protected abstract int DirectChildCountKernel { get; }

        internal sealed override int LeftDirectChildCount => LeftDirectChildCountKernel + 1;
        protected sealed override int DirectChildCount => DirectChildCountKernel + 2;


        [DisableDump]
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual bool? IsHollow => IsLambda? (bool?)true : null;

        [DisableDump]
        internal virtual IRecursionHandler RecursionHandler => null;

        DeclarerSyntax IStatementSyntax.Declarer => null;
        UsageTree IStatementSyntax.ToUsageValue() => new UsageTree {Left = true, Right = true};

        ValueSyntax IStatementSyntax.ToValueSyntax(BinaryTree binaryTree, BinaryTree rightAnchor) => this;

        ValueSyntax IStatementSyntax.Value => this;

        protected abstract Syntax GetDirectChildKernel(int index);

        protected sealed override Syntax GetDirectChild(int index)
            => index == 0
                ? LeftBracket
                : index == DirectChildCount - 1
                    ? RightBracket
                    : GetDirectChildKernel(index - 1);

        internal override Result<StatementSyntax[]> ToStatementsSyntax(BinaryTree target = null)
            => StatementSyntax.Create(target, this);

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
            if(type != null)
                return type.SmartUn<FunctionType>().IsHollow;

            return Type(context).SmartUn<FunctionType>().IsHollow;
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