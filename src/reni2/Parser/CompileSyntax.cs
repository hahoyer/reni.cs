using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Forms;
using hw.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Parser
{
    abstract class CompileSyntax : Syntax
    {
        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly FunctionCache<ContextBase, ResultCache> ResultCache =
            new FunctionCache<ContextBase, ResultCache>();

        internal CompileSyntax() { }

        internal CompileSyntax(int objectId)
            : base(objectId) {}

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
        internal override Checked<CompileSyntax> ToCompiledSyntax => this;

        internal virtual IRecursionHandler RecursionHandler => null;

        internal void AddToCacheForDebug(ContextBase context, ResultCache cacheItem)
            => ResultCache.Add(context, cacheItem);

        internal Result Result(ContextBase context) => context.Result(Category.All, this);

        internal BitsConst Evaluate(ContextBase context)
            => Result(context).Evaluate(context.RootContext.ExecutionContext);

        internal Result AtTokenResult(ContextBase context, Category category, CompileSyntax right)
        {
            var leftResultAsRef = context.ResultAsReference(category.Typed, this);
            return leftResultAsRef
                .Type
                .FindRecentCompoundView
                .AccessViaPositionExpression(category, right.Result(context))
                .ReplaceArg(leftResultAsRef);
        }

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

        internal CompileSyntax ReplaceArg(CompileSyntax value)
            => Visit(new ReplaceArgVisitor(value)) ?? this;

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }

        virtual internal ResultCache.IResultProvider FindSource(IContextReference ext, ContextBase context)
        {
            NotImplementedMethod(ext,context);
            return null;

        }
    }

    interface IRecursionHandler
    {
        Result Execute
            (
            ContextBase context,
            Category category,
            Category pendingCategory,
            CompileSyntax syntax,
            bool asReference);
    }

    sealed class ReplaceArgVisitor : DumpableObject, ISyntaxVisitor
    {
        readonly CompileSyntax _value;
        public ReplaceArgVisitor(CompileSyntax value) { _value = value; }
        CompileSyntax ISyntaxVisitor.Arg => _value;
    }

    interface ISyntaxVisitor
    {
        CompileSyntax Arg { get; }
    }
}