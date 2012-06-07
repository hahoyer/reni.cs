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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Struct
{
    sealed class FunctionBodyType : TypeBase, IFunctionFeature, IContextReference
    {
        [EnableDump]
        readonly Structure _structure;
        [EnableDump]
        readonly FunctionSyntax _syntax;

        public FunctionBodyType(Structure structure, FunctionSyntax syntax)
        {
            _structure = structure;
            _syntax = syntax;
        }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return _structure; } }
        [DisableDump]
        internal override bool IsLambda { get { return true; } }
        [DisableDump]
        internal override bool IsDataLess { get { return _structure.IsDataLess; } }
        internal override void Search(SearchVisitor searchVisitor) { NotImplementedMethod(); }
        Size IContextReference.Size { get { return _structure.RefAlignParam.RefSize; } }
        internal override string DumpPrintText { get { return _syntax.DumpPrintText; } }

        protected override Size GetSize() { return _structure.RefAlignParam.RefSize; }

        [DisableDump]
        bool IFunctionFeature.IsImplicit { get { return _syntax.IsImplicit; } }
        [DisableDump]
        IContextReference IFunctionFeature.ObjectReference { get { return this; } }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType) { return Function(argsType).ApplyResult(category); }

        FunctionType Function(TypeBase argsType) { return _structure.Function(_syntax, argsType); }
    }
}