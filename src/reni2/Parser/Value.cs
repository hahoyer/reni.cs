using System.Collections;
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
    /// Static syntax items that represent a value
    /// </summary>
    abstract class Value : DumpableObject
        , ITree<Value>
    {
        class CacheContainer
        {
            public Value[] Children;
        }

        readonly CacheContainer Cache = new CacheContainer();



        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        protected Value(BinaryTree binaryTree) => BinaryTree = binaryTree;

        protected Value(int objectId, BinaryTree binaryTree)
            : base(objectId)
            => BinaryTree = binaryTree;

        internal BinaryTree BinaryTree {get;}

        [DisableDump]
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual bool? IsHollow => IsLambda ? (bool?) true : null;

        [DisableDump]
        internal virtual IRecursionHandler RecursionHandler => null;

        internal Value[] Children => Cache.Children?? (Cache.Children = GetChildren().ToArray());
        protected abstract IEnumerable<Value> GetChildren();

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

        internal Value ReplaceArg(Value value)
            => Visit(new ReplaceArgVisitor(value)) ?? this;

        internal virtual Value Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        internal IEnumerable<string> DeclarationOptions(ContextBase context)
            => Type(context).DeclarationOptions;

        int ITree<Value>.ChildrenCount => Children.Length;
        Value ITree<Value>.Child(int index) => Children[index];

    }

    interface IValuesScope
    {
        IDefaultScopeProvider DefaultScopeProvider { get; }
        bool IsDeclarationPart { get; }
    }
}