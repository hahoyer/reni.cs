using System;
using System.Collections.Generic;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Parser
{
    /// <summary>
    ///     Static syntax items that represent a value
    /// </summary>
    abstract class ValueSyntax : Syntax
    {
        internal new abstract class NoChildren : ValueSyntax
        {
            protected NoChildren(BinaryTree target)
                : base(target) { }

            protected NoChildren(int objectId, BinaryTree target)
                : base(objectId, target) { }

            [DisableDump]
            protected sealed override int LeftDirectChildCount => 0;

            [DisableDump]
            protected sealed override int DirectChildCount => 0;

            protected sealed override Syntax GetDirectChild(int index)
                => throw new Exception($"Unexpected call: {nameof(GetDirectChild)}({index})");
        }

        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        protected ValueSyntax(BinaryTree target)
            : base(target) { }

        protected ValueSyntax(int objectId, BinaryTree target)
            : base(objectId, target) { }

        [DisableDump]
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual bool? IsHollow => IsLambda? (bool?)true : null;

        [DisableDump]
        internal virtual IRecursionHandler RecursionHandler => null;

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

        protected override Result<CompoundSyntax> ToCompoundSyntaxHandler(BinaryTree target = null)
            => new StatementSyntax(null, target, this).ToCompoundSyntax();

        internal override StatementSyntax[] ToStatementsSyntax(BinaryTree target = null) 
            => T(new StatementSyntax(null, target, this));

    }
}