using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Parser
{
    abstract class Value : DumpableObject
    {
        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        internal Value() { }

        internal Value(int objectId)
            : base(objectId) { }

        [DisableDump]
        internal bool IsLambda => GetIsLambda();

        [DisableDump]
        internal virtual bool? Hllw => IsLambda ? (bool?) true : null;

        //[DebuggerHidden]
        internal virtual Result ResultForCache(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        protected virtual bool GetIsLambda() => false;

        [DisableDump]
        protected virtual IEnumerable<Value> DirectChildren { get { yield break; } }

        [DisableDump]
        public IEnumerable<Value> Closure
            => DirectChildren
                .Where(item => item != null)
                .SelectMany(item => item.Closure)
                .plus(this);

        internal virtual IRecursionHandler RecursionHandler => null;

        public SourcePart SourcePart => SourceStart.Span(SourceEnd);

        internal virtual SourcePosn SourceStart
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal virtual SourcePosn SourceEnd
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        internal void AddToCacheForDebug(ContextBase context, ResultCache cacheItem)
            => ResultCache.Add(context, cacheItem);

        internal Result Result(ContextBase context) => context.Result(Category.All, this);

        internal BitsConst Evaluate(ContextBase context)
            => Result(context).Evaluate(context.RootContext.ExecutionContext);

        internal CodeBase Code(ContextBase context)
        {
            var result = context.Result(Category.Code, this).Code;
            Tracer.Assert(result != null);
            return result;
        }

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

        internal virtual ResultCache.IResultProvider FindSource
            (IContextReference ext, ContextBase context)
        {
            NotImplementedMethod(ext, context);
            return null;
        }

        internal IEnumerable<string> DeclarationOptions(ContextBase context)
            => Type(context).DeclarationOptions;

    }

    interface IRecursionHandler
    {
        Result Execute
            (
            ContextBase context,
            Category category,
            Category pendingCategory,
            Value syntax,
            bool asReference);
    }

    sealed class ReplaceArgVisitor : DumpableObject, ISyntaxVisitor
    {
        readonly Value _value;
        public ReplaceArgVisitor(Value value) { _value = value; }
        Value ISyntaxVisitor.Arg => _value;
    }

    interface ISyntaxVisitor
    {
        Value Arg { get; }
    }
}