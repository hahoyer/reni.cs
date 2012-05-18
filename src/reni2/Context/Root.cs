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
using Reni.Basics;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Context
{
    sealed class Root : ContextBase
    {
        [DisableDump]
        readonly FunctionList _functions;
        [DisableDump]
        internal readonly IOutStream OutStream;


        internal Root(FunctionList functions, IOutStream outStream)
        {
            _functions = functions;
            OutStream = outStream;
        }

        [DisableDump]
        internal override Root RootContext { get { return this; } }

        internal override void Search(ContextSearchVisitor searchVisitor) { searchVisitor.Search(); }

        protected override Result CommonResult(Category category, CondSyntax condSyntax)
        {
            NotImplementedMethod(category, condSyntax);
            return null;
        }

        internal static RefAlignParam DefaultRefAlignParam { get { return new RefAlignParam(BitsConst.SegmentAlignBits, Size.Create(32)); } }

        internal FunctionInstance FunctionInstance(Structure structure, CompileSyntax body, TypeBase argsType)
        {
            var alignedArgsType = argsType.UniqueAlign(DefaultRefAlignParam.AlignBits);
            var functionInstance = _functions.Find(body, structure, alignedArgsType);
            return functionInstance;
        }
    }
}