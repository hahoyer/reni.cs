#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

#endregion

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
using Reni.Struct;
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

        [DisableDump]
        internal virtual bool IsImplicit { get { throw new NotImplementedException(); } }

        //[DebuggerHidden]
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
        internal BitsConst Evaluate(ContextBase context) { return Result(context).Evaluate(context.RootContext.OutStream); }

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

        bool? QuickIsDataLessStructureElement(ContextBase context)
        {
            var contextResult = context.QuickIsDataLess(this);
            if(contextResult != null)
                return contextResult;

            var type = FindResult(context, Category.Type).Type;
            if(type != null)
                return type.IsDataLessStructureElement(true);
            return null;
        }

        internal Result OperationResult(ContextBase context, Category category, ISearchTarget target)
        {
            return Type(context)
                .OperationResult<IPrefixFeature>(category, target);
        }

        internal bool? IsDataLessStructureElement(bool isQuick, ContextBase context)
        {
            var result = QuickIsDataLessStructureElement(context);
            if(result != null)
                return result;
            if(isQuick)
                return null;

            return Type(context).IsDataLessStructureElement(false);
        }

        internal ISimpleFeature SimpleFeature(Structure structure)
        {
            NotImplementedMethod(structure);
            return null;
        }

        internal Result SmartLocalReferenceResult(ContextBase context, Category category)
        {
            return Result(context, category.Typed)
                .SmartLocalReferenceResult();
        }
    }
}