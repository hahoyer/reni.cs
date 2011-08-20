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
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Feature;
using Reni.Struct;
using Reni.Syntax;

namespace Reni.Context
{
    internal sealed class Root : ContextBase
    {
        [DisableDump]
        private readonly FunctionList _functions;

        internal Root(FunctionList functions) { _functions = functions; }

        [DisableDump]
        internal override Root RootContext { get { return this; } }

        internal override void Search(SearchVisitor<IContextFeature> searchVisitor) { searchVisitor.SearchTypeBase(); }

        protected override Result CommonResult(Category category, CondSyntax condSyntax)
        {
            NotImplementedMethod(category, condSyntax);
            return null;
        }

        internal static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); } }

        internal Result CreateFunctionCall(Structure structure, Category category, CompileSyntax body, Result argsResult)
        {
            Tracer.Assert(argsResult.HasType);
            var alignedArgsResult = argsResult.Align(DefaultRefAlignParam.AlignBits);
            var functionInstance = _functions.Find(body, structure, alignedArgsResult.Type);
            return functionInstance.CreateCall(category, alignedArgsResult);
        }
    }
}