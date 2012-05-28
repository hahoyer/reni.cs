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
    abstract class FunctionInstanceType : TypeBase
    {
        protected readonly FunctionType Parent;
        [Node]
        [EnableDump]
        readonly CompileSyntax _body;

        readonly SimpleCache<CodeBase> _bodyCodeCache;

        protected FunctionInstanceType(FunctionType parent, CompileSyntax body)
        {
            _body = body;
            Parent = parent;
            _bodyCodeCache = new SimpleCache<CodeBase>(ObtainBodyCode);
        }

        [Node]
        [DisableDump]
        internal CodeBase BodyCode { get { return _bodyCodeCache.Value; } }
        [DisableDump]
        protected RefAlignParam RefAlignParam { get { return Context.RefAlignParam; } }
        [Node]
        [DisableDump]
        string Description { get { return _body.DumpShort(); } }
        [Node]
        [DisableDump]
        protected virtual Size FrameSize { get { return Parent.ArgsType.Size + CodeArgs.Size; } }
        [Node]
        [DisableDump]
        internal CodeArgs CodeArgs { get { return ApplyResult(Category.CodeArgs).CodeArgs; } }
        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        protected abstract FunctionId FunctionId { get; }
        protected override Size GetSize() { return RefAlignParam.RefSize; }
        protected abstract ContextBase Context { get; }

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
            var result = ApplyResult(localCategory);

            if(category.HasArgs)
                result.CodeArgs = CodeArgs.Arg();
            if(category.HasCode)
                result.Code = Parent.ArgCode()
                    .Call(FunctionId, result.Size);
            return result;
        }

        protected Result ApplyResult(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = FunctionId.Index == 0 && category.HasCode;
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
                .AddToReference(FrameSize * -1)
                .Dereference(refAlignParam.RefSize);
        }

        internal void EnsureBodyCode() { _bodyCodeCache.Ensure(); }

        CodeBase ObtainBodyCode()
        {
            if(IsStopByObjectIdActive)
                return null;
            var category = Category.Code;
            var foreignRefsRef = CodeBase.FrameRef(RefAlignParam);
            var visitResult = ApplyResult(category);
            var result = visitResult
                .ReplaceRefsForFunctionBody(RefAlignParam.RefSize, foreignRefsRef);
            if(Parent.ArgsType.IsDataLess)
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

    sealed class FunctionId
    {
        public static FunctionId Getter(int index) { return new FunctionId(index, true); }
        public static FunctionId Setter(int index) { return new FunctionId(index, false); }
        
        internal readonly int Index;
        internal readonly bool IsGetter;

        FunctionId(int index, bool isGetter)
        {
            Index = index;
            IsGetter = isGetter;
        }

        public override string ToString() { return Index.ToString() + "." + (IsGetter ? "get" : "set"); }
    }
}