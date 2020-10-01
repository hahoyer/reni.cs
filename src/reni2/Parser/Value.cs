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
    /// Static syntax items that represent a value
    /// </summary>
    abstract class Value : DumpableObject
    {
        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        protected Value(Syntax syntax) => Syntax = syntax;

        protected Value(int objectId, Syntax syntax)
            : base(objectId)
            => Syntax = syntax;

        internal Syntax Syntax {get;}

        [DisableDump]
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual bool? IsHollow => IsLambda ? (bool?) true : null;

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

        internal Value ReplaceArg(Value value)
            => Visit(new ReplaceArgVisitor(value)) ?? this;

        internal virtual Value Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        internal IEnumerable<string> DeclarationOptions(ContextBase context)
            => Type(context).DeclarationOptions;
    }
}