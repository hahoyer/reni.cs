#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.Type
{
    sealed class FunctionFeature : ReniObject, IFunctionFeature, IContextReference
    {
        readonly Structure _structure;
        readonly FunctionSyntax _syntax;
        public FunctionFeature(Structure structure, FunctionSyntax syntax)
        {
            _structure = structure;
            _syntax = syntax;
        }

        [DisableDump]
        RefAlignParam RefAlignParam { get { return _structure.RefAlignParam; } }

        IContextReference IFunctionFeature.ObjectReference { get { return this; } }
        bool IFunctionFeature.IsImplicit { get { return _syntax.IsImplicit; } }
        Size IContextReference.Size { get { return RefAlignParam.RefSize; } }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return Function(argsType).ApplyResult(category); }
        FunctionType Function(TypeBase argsType) { return _structure.Function(_syntax, argsType); }

    }
}