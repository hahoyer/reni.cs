using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Context;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Parser
{
    abstract class Value : DumpableObject
    {
        internal Syntax Syntax { get; }

        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        protected Value(Syntax syntax) { Syntax = syntax; }

        protected Value(int objectId, Syntax syntax)
            : base(objectId) { Syntax = syntax; }

        [DisableDump]
        internal virtual bool IsLambda => false;

        [DisableDump]
        internal virtual bool? Hllw => IsLambda ? (bool?) true : null;

        //[DebuggerHidden]
        internal virtual Result ResultForCache(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        [DisableDump]
        internal virtual IRecursionHandler RecursionHandler => null;

        internal SourcePart SourcePart => Syntax.SourcePart;

        internal void AddToCacheForDebug(ContextBase context, ResultCache cacheItem)
            => ResultCache.Add(context, cacheItem);

        internal Result Result(ContextBase context) => context.Result(Category.All, this);

        internal BitsConst Evaluate(ContextBase context)
            => Result(context).Evaluate(context.RootContext.ExecutionContext);

        //[DebuggerHidden]
        internal TypeBase Type(ContextBase context) => context.Result(Category.Type, this)?.Type;

        internal bool HllwStructureElement(ContextBase context)
        {
            var result = Hllw;
            if(result != null)
                return result.Value;

            var type = context.TypeIfKnown(this);
            if(type != null)
                return type.SmartUn<FunctionType>().Hllw;

            return Type(context).SmartUn<FunctionType>().Hllw;
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