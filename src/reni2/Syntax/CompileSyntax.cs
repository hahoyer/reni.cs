//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Syntax
{
    abstract class CompileSyntax : ReniParser.ParsedSyntax
    {
        // Used for debug only
        [DisableDump]
        [Node("Cache")]
        internal readonly DictionaryEx<ContextBase, object> ResultCache = new DictionaryEx<ContextBase, object>();

        internal CompileSyntax(TokenData token)
            : base(token) { }

        internal CompileSyntax(TokenData token, int objectId)
            : base(token, objectId) { }

        [DisableDump]
        internal bool IsLambda { get { return GetIsLambda(); } }

        [DisableDump]
        internal virtual string DumpPrintText
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }

        [DebuggerHidden]
        internal virtual Result ObtainResult(ContextBase context, Category category)
        {
            NotImplementedMethod(context, category);
            return null;
        }

        protected virtual bool GetIsLambda() { return false; }
        internal override ReniParser.ParsedSyntax CreateSyntaxOrDeclaration(Defineable tokenClass, TokenData token, ReniParser.ParsedSyntax right) { return new ExpressionSyntax(tokenClass, this, token, right.ToCompiledSyntaxOrNull()); }
        internal override ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData token, TokenData rightToken) { return this; }
        internal override CompileSyntax ToCompiledSyntax() { return this; }
        internal void AddToCacheForDebug(ContextBase context, object cacheItem) { ResultCache.Add(context, cacheItem); }
        internal Result Result(ContextBase context) { return Result(context, Category.All); }
        //[DebuggerHidden]
        internal Result Result(ContextBase context, Category category) { return context.UniqueResult(category, this); }
        Result FindResult(ContextBase context, Category category) { return context.FindResult(category, this); }
        internal BitsConst Evaluate(ContextBase context) { return Result(context).Evaluate(); }
        internal Result ResultAsReference(ContextBase context, Category category) { return context.UniqueResult(category.Typed, this).LocalReferenceResult(context.RefAlignParam); }

        internal Result AtTokenResult(ContextBase context, Category category, CompileSyntax right)
        {
            var leftResultAsRef = ResultAsReference(context, category.Typed);
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

        internal virtual bool? QuickIsDereferencedDataLess(ContextBase context)
        {
            var contextResult = context.QuickIsDataLess(this);
            if (contextResult != null)
                return contextResult;
            
            var type = FindResult(context, Category.Type).Type;
            if(type != null)
                return type.IsDereferencedDataLess(true);
            return null;
        }

        internal Result OperationResult<TFeature>(ContextBase context, Category category, Defineable defineable)
            where TFeature : class, IFeature
        {
            var trace = ObjectId == -21 && context.ObjectId == 2 && defineable.ObjectId == 31
                || ObjectId == 241 && context.ObjectId == 4 && defineable.ObjectId == 19;
            StartMethodDump(trace, context, category, defineable);
            try
            {
                BreakExecution();
                var targetType = Type(context);
                Dump("targetType", targetType);
                BreakExecution();
                var operationResult = targetType.OperationResult<TFeature>(category, defineable, context.RefAlignParam);
                if(operationResult == null)
                    return (null);

                if(!category.HasCode && !category.HasArgs)
                    return ReturnMethodDump(operationResult, true);

                if (!operationResult.HasArg)
                {
                    return ReturnMethodDump(operationResult, true);
                }

                Dump("operationResult", operationResult);
                BreakExecution();
                var targetResult = ResultAsReference(context, category.Typed);
                Dump("targetResult", targetResult);
                BreakExecution();
                var result = operationResult.ReplaceArg(targetResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        
        internal bool? IsDereferencedDataLess(bool isQuick, ContextBase context)
        {
            var result = QuickIsDereferencedDataLess(context);
            if (result != null)
                return result;
            if (isQuick)
                return null;

            return Type(context).IsDereferencedDataLess(true);
        }
    }
}