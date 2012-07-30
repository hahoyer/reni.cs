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
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Code;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Context
{
    sealed class Root : ContextBase
    {
        [DisableDump]
        readonly FunctionList _functions;
        [DisableDump]
        internal readonly IExecutionContext ExecutionContext;

        readonly SimpleCache<BitType> _bitCache;
        readonly SimpleCache<VoidType> _voidCache;

        internal BitType BitType { get { return _bitCache.Value; } }
        internal VoidType VoidType { get { return _voidCache.Value; } }

        internal Root(FunctionList functions, IExecutionContext executionContext)
        {
            _functions = functions;
            ExecutionContext = executionContext;
            _bitCache = new SimpleCache<BitType>(() => new BitType(this));
            _voidCache = new SimpleCache<VoidType>(() => new VoidType(this));
        }

        [DisableDump]
        internal override Root RootContext { get { return this; } }

        internal static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); } }

        internal FunctionType FunctionInstance(Structure structure, FunctionSyntax body, TypeBase argsType)
        {
            var alignedArgsType = argsType.UniqueAlign;
            var functionInstance = _functions.Find(body, structure, alignedArgsType);
            return functionInstance;
        }
        internal Result VoidResult(Category category) { return VoidType.Result(category); }

        internal Result ConcatPrintResult(Category category, int count, Func<int, Result> elemResults)
        {
            var result = VoidResult(category);
            if(!(category.HasCode || category.HasArgs))
                return result;

            if(category.HasCode)
                result.Code = CodeBase.DumpPrintText("(");

            for(var i = 0; i < count; i++)
            {
                var elemResult = elemResults(i);
                result.IsDirty = true;
                if(category.HasCode)
                {
                    if(i > 0)
                        result.Code = result.Code.Sequence(CodeBase.DumpPrintText(", "));
                    result.Code = result.Code.Sequence(elemResult.Code);
                }
                if(category.HasArgs)
                    result.CodeArgs = result.CodeArgs.Sequence(elemResult.CodeArgs);
                result.IsDirty = false;
            }
            if(category.HasCode)
                result.Code = result.Code.Sequence(CodeBase.DumpPrintText(")"));
            return result;
        }
    }
}