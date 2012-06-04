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
using Reni.Context;

namespace Reni.Type
{
    sealed class ConcatArraysFeature : TypeBase, IFunctionFeature
    {
        [EnableDump]
        readonly Array _type;

        public ConcatArraysFeature(Array type)
        {
            _type = type;
        }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }

        Result IFunctionFeature.ApplyResult(Category category, TypeBase argsType)
        {
            var newCount = argsType.ArrayElementCount;
            var newElementResult = argsType
                .Conversion(category, argsType.IsArray ? _type.Element.UniqueArray(newCount) : _type.Element);
            return _type
                .Element
                .UniqueArray(_type.Count + newCount)
                .Result
                (category
                 , () => newElementResult.Code.Sequence(_type.DereferencedReferenceCode())
                 , () => newElementResult.CodeArgs + CodeArgs.Arg()
                );
        }

        [DisableDump]
        bool IFunctionFeature.IsImplicit { get { return false; } }
        [DisableDump]
        IContextReference IFunctionFeature.ObjectReference
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