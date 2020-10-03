using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Helper;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Parser
{
    /// <summary>
    ///     Static syntax items that represent a value
    /// </summary>
    abstract class Syntax : DumpableObject, ITree<Syntax>
    {
        class CacheContainer
        {
            public Syntax[] Children;
        }

        internal readonly BinaryTree BinaryTree;

        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        readonly CacheContainer Cache = new CacheContainer();

        protected Syntax(BinaryTree binaryTree) => BinaryTree = binaryTree;

        protected Syntax(int objectId, BinaryTree binaryTree)
            : base(objectId)
            => BinaryTree = binaryTree;

        [DisableDump]
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual bool? IsHollow => IsLambda? (bool?)true : null;

        [DisableDump]
        internal virtual IRecursionHandler RecursionHandler => null;

        internal Syntax[] Children => Cache.Children ?? (Cache.Children = GetChildren().ToArray());
        Syntax ITree<Syntax>.Child(int index) => Children[index];

        int ITree<Syntax>.ChildrenCount => Children.Length;
        protected abstract IEnumerable<Syntax> GetChildren();

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

        internal Syntax ReplaceArg(Syntax syntax)
            => Visit(new ReplaceArgVisitor(syntax)) ?? this;

        internal virtual Syntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        internal IEnumerable<string> DeclarationOptions(ContextBase context)
            => Type(context).DeclarationOptions;
    }
}