using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
using hw.Forms;
using hw.Helper;
using hw.Parser;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.ReniParser;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Syntax
{
    abstract class CompileSyntax : ParsedSyntax
    {
        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        readonly FunctionCache<ContextBase, object> ResultCache = new FunctionCache<ContextBase, object>();

        internal CompileSyntax(TokenData token)
            : base(token)
        {}

        internal CompileSyntax(TokenData token, int objectId)
            : base(token, objectId)
        {}

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

        [DisableDump]
        internal virtual bool IsImplicit { get { throw new NotImplementedException(); } }

        //[DebuggerHidden]
        internal virtual Result ObtainResult(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        protected virtual bool GetIsLambda() { return false; }
        internal override ParsedSyntax CreateSyntaxOrDeclaration(Definable tokenClass, TokenData token, ParsedSyntax right)
        {
            return new ExpressionSyntax(tokenClass, this, token, right.ToCompiledSyntaxOrNull());
        }
        internal override ParsedSyntax SurroundedByParenthesis(TokenData token, TokenData rightToken) { return this; }
        internal override CompileSyntax ToCompiledSyntax() { return this; }
        internal void AddToCacheForDebug(ContextBase context, object cacheItem) { ResultCache.Add(context, cacheItem); }
        internal Result Result(ContextBase context) { return Result(context, Category.All); }
        //[DebuggerHidden]
        internal Result Result(ContextBase context, Category category) { return context.UniqueResult(category, this); }
        Result FindResult(ContextBase context, Category category) { return context.FindResult(category, this); }
        internal BitsConst Evaluate(ContextBase context)
        {
            return Result(context).Evaluate(context.RootContext.ExecutionContext);
        }

        internal Result AtTokenResult(ContextBase context, Category category, CompileSyntax right)
        {
            var leftResultAsRef = context.ResultAsReference(category.Typed, this);
            var rightResult = right.Result(context);
            return leftResultAsRef
                .Type
                .FindRecentStructure
                .AccessViaThisReference(category, rightResult)
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

            result = context.QuickHllw(this);
            if(result != null)
                return result.Value;

            var type = FindResult(context, Category.Type).Type;
            if(type != null)
                return type.SmartUn<FunctionType>().Hllw;

            return Type(context).SmartUn<FunctionType>().Hllw;
        }

        internal Result PointerKindResult(ContextBase context, Category category)
        {
            return Result(context, category.Typed).LocalPointerKindResult;
        }

        internal Result SmartUnFunctionedReferenceResult(ContextBase context, Category category)
        {
            var result = Result(context, category.Typed);
            if(result == null)
                return null;
            return result
                .SmartUn<FunctionType>().LocalPointerKindResult;
        }

        internal virtual Result ObtainPendingResult(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }
    }
}