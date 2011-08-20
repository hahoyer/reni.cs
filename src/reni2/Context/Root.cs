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
    internal sealed class Root : ReniObject, IContextItem
    {
        private readonly FunctionList _functions;

        internal Root(FunctionList functions) { _functions = functions; }

        RefAlignParam IContextItem.RefAlignParam { get { return DefaultRefAlignParam; } }
        void IContextItem.Search(SearchVisitor<IContextFeature> searchVisitor, ContextBase parent) { }

        private static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); } }

        [DisableDump]
        private FunctionList Functions { get { return _functions; } }

        internal Result CreateFunctionCall(Structure structure, Category category, CompileSyntax body, Result argsResult)
        {
            Tracer.Assert(argsResult.HasType);
            var alignedArgsResult = argsResult.Align(DefaultRefAlignParam.AlignBits);
            var functionInstance = Functions.Find(body, structure, alignedArgsResult.Type);
            return functionInstance.CreateCall(category, alignedArgsResult);
        }

        string IDumpShortProvider.DumpShort() { return DumpShort(); }
    }
}