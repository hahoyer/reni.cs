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
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Code;
using Reni.Type;

namespace Reni.Context
{
    sealed class Function : Child, IReferenceInCode
    {
        [Node]
        internal readonly TypeBase ArgsType;
        internal Function(ContextBase parent, TypeBase argsType)
            : base(parent) { ArgsType = argsType; }

        RefAlignParam IReferenceInCode.RefAlignParam { get { return RefAlignParam; } }

        internal override Function ObtainRecentFunctionContext() { return this; }

        protected override Result ObjectResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
        internal Result CreateArgsReferenceResult(Category category) { return ArgsType.ReferenceInCode(category, this); }
    }
}