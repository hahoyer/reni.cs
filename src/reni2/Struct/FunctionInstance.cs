#region Copyright (C) 2012

// 
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    abstract class FunctionInstance : ReniObject
    {
        internal abstract FunctionId FunctionId { get; }

        [Node]
        [EnableDump]
        readonly TypeBase _argsType;

        [Node]
        [EnableDump]
        readonly CompileSyntax _body;

        readonly SimpleCache<ContextBase> _contextCache;
        readonly SimpleCache<CodeBase> _bodyCodeCache;

        protected FunctionInstance([NotNull]CompileSyntax body, TypeBase argsType, Func<ContextBase> getContext)
        {
            _argsType = argsType;
            _body = body;
            _bodyCodeCache = new SimpleCache<CodeBase>(ObtainBodyCode);
            _contextCache = new SimpleCache<ContextBase>(getContext);
        }

        [Node]
        [DisableDump]
        internal CodeBase BodyCode { get { return _bodyCodeCache.Value; } }
        [DisableDump]
        RefAlignParam RefAlignParam { get { return Context.RefAlignParam; } }
        [DisableDump]
        ContextBase Context { get { return _contextCache.Value; } }
        [Node]
        [DisableDump]
        string Description { get { return _body.DumpShort(); } }
        [Node]
        [DisableDump]
        Size FrameSize { get { return _argsType.Size + CodeArgs.Size; } }
        [Node]
        [DisableDump]
        internal CodeArgs CodeArgs { get { return Result(Category.CodeArgs).CodeArgs; } }

        internal Code.Container Serialize()
        {
            try
            {
                return new Code.Container(BodyCode, Description, FunctionId, FrameSize);
            }
            catch(UnexpectedVisitOfPending)
            {
                return Code.Container.UnexpectedVisitOfPending;
            }
        }
        internal Result CallResult(Category category)
        {
            var localCategory = category - Category.CodeArgs - Category.Code;
            if(category.HasCode)
                localCategory |= Category.Size;
            return Result(localCategory);
        }

        protected Result Result(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = ObjectId == -10 && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var rawResult = _body.Result(Context, category.Typed).Clone();
                Dump("rawResult", rawResult);
                BreakExecution();
                var postProcessedResult = rawResult
                    .AutomaticDereference()
                    .Align(RefAlignParam.AlignBits)
                    .LocalBlock(category);

                Dump("postProcessedResult", postProcessedResult);
                BreakExecution();
                var result = postProcessedResult
                    .ReplaceAbsolute(Context.FindRecentFunctionContextObject, CreateContextRefCode, CodeArgs.Void);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        CodeBase CreateContextRefCode()
        {
            var refAlignParam = RefAlignParam;
            return CodeBase
                .FrameRef(refAlignParam)
                .AddToReference(refAlignParam, FrameSize * -1)
                .Dereference(refAlignParam, refAlignParam.RefSize);
        }

        internal void EnsureBodyCode() { _bodyCodeCache.Ensure(); }

        CodeBase ObtainBodyCode()
        {
            if(IsStopByObjectIdActive)
                return null;
            var category = Category.Code;
            var foreignRefsRef = CodeBase.FrameRef(RefAlignParam);
            var visitResult = Result(category);
            var result = visitResult
                .ReplaceRefsForFunctionBody(RefAlignParam, foreignRefsRef);
            if(_argsType.IsDataLess)
                result.Code = result.Code.TryReplacePrimitiveRecursivity(FunctionId);
            return result.Code;
        }

        public string DumpFunction()
        {
            var result = "\n";
            result += "body=" + _body.DumpShort();
            result += "\n";
            return result;
        }
    }

    sealed class GetterFunctionInstance : FunctionInstance
    {
        readonly FunctionId _functionId;
        public GetterFunctionInstance(int index, CompileSyntax body, TypeBase argsType, Func<ContextBase> getContext)
            : base(body, argsType, getContext) { _functionId = new FunctionId {Index = index, IsGetter = true}; }

        internal TypeBase ReturnType { get { return Result(Category.Type).Type; } }
        internal override FunctionId FunctionId { get { return _functionId; } }
    }

    sealed class SetterFunctionInstance : FunctionInstance
    {
        readonly FunctionId _functionId;
        public SetterFunctionInstance(int index, CompileSyntax body, TypeBase argsType, Func<ContextBase> getContext)
            : base(body, argsType, getContext) { _functionId = new FunctionId {Index = index, IsGetter = false}; }

        internal override FunctionId FunctionId { get { return _functionId; } }
    }

    sealed class FunctionId
    {
        public int Index;
        public bool IsGetter;
        public override string ToString() { return (IsGetter ? "Get" : "Set") + "." + Index; }
    }
}