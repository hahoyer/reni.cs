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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    /// <summary>
    ///     Instance of a function to compile
    /// </summary>
    [Serializable]
    sealed class FunctionInstance : ReniObject
    {
        [Node]
        [EnableDump]
        readonly TypeBase _args;

        [Node]
        [EnableDump]
        readonly CompileSyntax _body;

        [Node]
        [EnableDump]
        readonly Structure _structure;

        [EnableDump]
        readonly int _index;

        readonly SimpleCache<CodeBase> _bodyCodeCache;

        /// <summary>
        ///     Initializes a new instance of the FunctionInstance class.
        /// </summary>
        /// <param name="index"> The index. </param>
        /// <param name="body"> The body. </param>
        /// <param name="structure"> The context. </param>
        /// <param name="args"> The args. </param>
        /// created 03.01.2007 21:19
        internal FunctionInstance(int index, CompileSyntax body, Structure structure, TypeBase args)
            : base(index)
        {
            _index = index;
            _body = body;
            _structure = structure;
            _args = args;
            _bodyCodeCache = new SimpleCache<CodeBase>(ObtainBodyCode);
            StopByObjectId(-10);
        }

        [Node]
        TypeBase Type { get { return Result(Category.Type).Type; } }

        [Node]
        [DisableDump]
        internal CodeBase BodyCode { get { return _bodyCodeCache.Value; } }

        [Node]
        [DisableDump]
        CodeArgs CodeArgs
        {
            get
            {
                if(IsStopByObjectIdActive)
                    return null;
                return Result(Category.CodeArgs).CodeArgs;
            }
        }

        internal void EnsureBodyCode() { _bodyCodeCache.Ensure(); }

        [Node]
        [DisableDump]
        Size FrameSize { get { return _args.Size + CodeArgs.Size; } }

        [Node]
        [DisableDump]
        string Description { get { return _body.DumpShort(); } }

        public Result Call(Category category)
        {
            var trace = ObjectId == -10 && (category.HasCode || category.HasArgs);
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var localCategory = category;
                if(category.HasCode)
                    localCategory = (localCategory - Category.Code) | Category.Size;
                var result = Result(localCategory).Clone();
                if(category.HasArgs)
                    result.CodeArgs += CodeArgs.Arg();

                if(category.HasCode)
                    result.Code = ArgsForFunction.Call(_index, result.Size);

                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        [DisableDump]
        CodeBase ArgsForFunction { get { return CodeArgs.ToCode().Sequence(_args.ArgCode()); } }

        CodeBase ObtainBodyCode()
        {
            if(IsStopByObjectIdActive)
                return null;
            var category = Category.Code;
            var refAlignParam = _structure.UniqueContext.RefAlignParam;
            var foreignRefsRef = CodeBase.FrameRef(refAlignParam);
            var visitResult = Result(category);
            var result = visitResult
                .ReplaceRefsForFunctionBody(refAlignParam, foreignRefsRef);
            if(_args.IsDataLess)
                result.Code = result.Code.TryReplacePrimitiveRecursivity(_index);
            return result.Code;
        }

        Result Result(Category category)
        {
            if(IsStopByObjectIdActive)
                return null;

            var trace = ObjectId == -10 && category.HasCode;
            StartMethodDump(trace, category);
            try
            {
                BreakExecution();
                var functionContext = _structure.UniqueContext.UniqueFunctionContext(_args);
                Dump("functionContext", functionContext);
                var rawResult = _body.Result(functionContext, category.Typed).Clone();
                Dump("rawResult", rawResult);
                BreakExecution();
                var postProcessedResult = rawResult
                    .AutomaticDereference()
                    .Align(_structure.RefAlignParam.AlignBits)
                    .LocalBlock(category);

                Dump("postProcessedResult", postProcessedResult);
                BreakExecution();
                var result = postProcessedResult
                    .ReplaceAbsolute(functionContext.FindRecentFunctionContextObject, CreateContextRefCode, CodeArgs.Void);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        CodeBase CreateContextRefCode()
        {
            var refAlignParam = _structure.UniqueContext.RefAlignParam;
            return CodeBase
                .FrameRef(refAlignParam)
                .AddToReference(refAlignParam, FrameSize * -1)
                .Dereference(refAlignParam, refAlignParam.RefSize);
        }

        internal Code.Container Serialize(bool isInternal)
        {
            try
            {
                return new Code.Container(BodyCode, Description, FrameSize);
            }
            catch(UnexpectedVisitOfPending)
            {
                return Code.Container.UnexpectedVisitOfPending;
            }
        }

        public string DumpFunction()
        {
            var result = "\n";
            result += "index=" + _index;
            result += "\n";
            result += "body=" + _body.DumpShort();
            result += "\n";
            result += "args=" + _args.Dump();
            result += "\n";
            result += "context=" + _structure.Dump();
            result += "\n";
            result += "type=" + Type.Dump();
            result += "\n";
            return result;
        }
    }
}