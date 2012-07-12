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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    abstract class FunctionInstance : ReniObject
    {
        protected readonly FunctionType Parent;
        [Node]
        [EnableDump]
        readonly CompileSyntax _body;

        readonly SimpleCache<CodeBase> _bodyCodeCache;
        readonly SimpleCache<ContextBase> _contextCache;

        protected FunctionInstance(FunctionType parent, CompileSyntax body)
        {
            _body = body;
            Parent = parent;
            _bodyCodeCache = new SimpleCache<CodeBase>(ObtainBodyCode);
            _contextCache = new SimpleCache<ContextBase>(ObtainCache);
        }

        [Node]
        [DisableDump]
        internal CodeBase BodyCode { get { return _bodyCodeCache.Value; } }
        [Node]
        [DisableDump]
        string Description { get { return _body.DumpShort(); } }
        [Node]
        [DisableDump]
        protected Size FrameSize { get { return Parent.Size + RelevantValueSize; } }
        [DisableDump]
        protected Size ArgsPartSize { get { return Parent.ArgsType.Size + RelevantValueSize; } }
        [DisableDump]
        abstract protected Size RelevantValueSize { get; }
        [Node]
        [DisableDump]
        internal CodeArgs CodeArgs { get { return Result(Category.CodeArgs).CodeArgs; } }
        protected abstract FunctionId FunctionId { get; }
        [Node]
        [DisableDump]
        ContextBase Context { get { return _contextCache.Value; } }

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
            var result = Result(localCategory);

            if(category.HasArgs)
                result.CodeArgs = CodeArgs.Arg();
            if(category.HasCode)
                result.Code = CallType
                    .ArgCode
                    .Call(FunctionId, result.Size);
            return result;
        }

        protected virtual TypeBase CallType { get { return Parent; } }

        Result Result(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = FunctionId.Index == -1 && !FunctionId.IsGetter && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var rawResult = _body.Result(Context, category.Typed).Clone();

                Dump("rawResult", rawResult);
                BreakExecution();
                var postProcessedResult = rawResult
                    .AutomaticDereferenceResult()
                    .Align(Root.DefaultRefAlignParam.AlignBits)
                    .LocalBlock(category);

                Dump("postProcessedResult", postProcessedResult);
                BreakExecution();
                var result = postProcessedResult
                    .ReplaceAbsolute(Context.FindRecentFunctionContextObject, CreateContextRefCode, CodeArgs.Void);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }

        CodeBase CreateContextRefCode()
        {
            return CodeBase
                .FrameRef(Root.DefaultRefAlignParam)
                .ReferencePlus(ArgsPartSize);
        }

        internal void EnsureBodyCode() { _bodyCodeCache.Ensure(); }

        bool _isObtainBodyCodeActive;

        CodeBase ObtainBodyCode()
        {
            if(_isObtainBodyCodeActive || IsStopByObjectIdActive)
                return null;

            try
            {
                _isObtainBodyCodeActive = true;
                var foreignRefsRef = CreateContextRefCode();
                var visitResult = Result(Category.Code | Category.CodeArgs);
                var result = visitResult
                    .ReplaceRefsForFunctionBody(Root.DefaultRefAlignParam.RefSize, foreignRefsRef)
                    .Code;
                if(Parent.ArgsType.IsDataLess)
                    return result.TryReplacePrimitiveRecursivity(FunctionId);
                return result;
            }
            finally
            {
                _isObtainBodyCodeActive = false;
            }
        }

        public string DumpFunction()
        {
            var result = "\n";
            result += "body=" + _body.DumpShort();
            result += "\n";
            return result;
        }

        ContextBase ObtainCache() { return Parent.CreateSubContext(!IsGetter); }
        bool IsGetter { get { return FunctionId.IsGetter; } }
    }
}