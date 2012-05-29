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

namespace Reni.Type
{
    sealed class ConcatArraysFeature : TypeBase, IFunctionalFeature
    {
        [EnableDump]
        readonly Array _type;
        readonly RefAlignParam _refAlignParam;

        public ConcatArraysFeature(Array type, RefAlignParam refAlignParam)
        {
            _type = type;
            _refAlignParam = refAlignParam;
        }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        protected override Size GetSize() { return _refAlignParam.RefSize; }

        Result IFunctionalFeature.ApplyResult(Category category, TypeBase argsType)
        {
            var newCount = argsType.ArrayElementCount;
            var newElementResult = argsType
                .Conversion(category, argsType.IsArray ? _type.Element.UniqueArray(newCount) : _type.Element);
            return _type
                .Element
                .UniqueArray(_type.Count + newCount)
                .Result
                (category
                 , () => newElementResult.Code.Sequence(_type.DereferencedReferenceCode(_refAlignParam))
                 , () => newElementResult.CodeArgs + CodeArgs.Arg()
                );
        }

        [DisableDump]
        bool IFunctionalFeature.IsImplicit { get { return false; } }
        [DisableDump]
        IReferenceInCode IFunctionalFeature.ObjectReference
        {
            get
            {
                NotImplementedMethod();
                return null;
            }
        }
        internal override void Search(SearchVisitor searchVisitor) { NotImplementedMethod(); }
    }
}