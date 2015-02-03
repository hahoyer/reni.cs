using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Scanner;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniParser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.ReniSyntax
{
    abstract class CompileSyntax : Syntax
    {
        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        readonly FunctionCache<ContextBase, object> _resultCache = new FunctionCache<ContextBase, object>();

        internal CompileSyntax(SourcePart token)
            : base(token) {}

        internal CompileSyntax(SourcePart token, int objectId)
            : base(token, objectId) {}

        [DisableDump]
        internal bool IsLambda { get { return GetIsLambda(); } }

        [DisableDump]
        internal virtual bool? Hllw { get { return IsLambda ? (bool?) true : null; } }

        [DisableDump]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }


        //[DebuggerHidden]
        internal virtual Result ResultForCache(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        protected virtual bool GetIsLambda() => false;

        internal override CompileSyntax ToCompiledSyntax { get { return this; } }
        internal void AddToCacheForDebug(ContextBase context, object cacheItem) => _resultCache.Add(context, cacheItem);
        internal Result Result(ContextBase context) => Result(context, Category.All);
        //[DebuggerHidden]
        internal Result Result(ContextBase context, Category category) => context.Result(category, this);

        internal BitsConst Evaluate(ContextBase context) => Result(context).Evaluate(context.RootContext.ExecutionContext);

        internal Result AtTokenResult(ContextBase context, Category category, CompileSyntax right)
        {
            var leftResultAsRef = context.ResultAsReference(category.Typed, this);
            var rightResult = right.Result(context);
            return leftResultAsRef
                .Type
                .FindRecentCompoundView
                .AccessViaObjectPointer(category, rightResult)
                .ReplaceArg(leftResultAsRef);
        }

        internal CodeBase Code(ContextBase context)
        {
            var result = Result(context, Category.Code).Code;
            Tracer.Assert(result != null);
            return result;
        }

        [DebuggerHidden]
        internal TypeBase Type(ContextBase context)
        {
            var result = Result(context, Category.Type).Type;
            Tracer.Assert(result != null);
            return result;
        }

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

        internal Result SmartUnFunctionedReferenceResult(ContextBase context, Category category)
        {
            var result = Result(context, category.Typed);
            return result?.SmartUn<FunctionType>().LocalReferenceResult;
        }

        internal virtual Result ObtainPendingResult(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        public CompileSyntax ReplaceArg(CompileSyntax value) => Visit(new ReplaceArgVisitor(value));

        internal virtual CompileSyntax Visit(ISyntaxVisitor visitor)
        {
            NotImplementedMethod(visitor);
            return null;
        }
    }

    sealed class ReplaceArgVisitor : DumpableObject, ISyntaxVisitor
    {
        readonly CompileSyntax _value;
        public ReplaceArgVisitor(CompileSyntax value) { _value = value; }
        CompileSyntax ISyntaxVisitor.Arg { get { return _value; } }
    }

    interface ISyntaxVisitor
    {
        CompileSyntax Arg { get; }
    }
}