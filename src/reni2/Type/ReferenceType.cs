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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    sealed class ReferenceType 
        : TypeBase
          , ISearchPath<ISuffixFeature, Array>
    {
        readonly TypeBase _elementType;
        readonly int _count;

        internal ReferenceType(TypeBase elementType, int count)
        {
            _elementType = elementType;
            _count = count;
            AssertValid();
        }

        [EnableDump]
        TypeBase ElementType { get { return _elementType; } }
        [EnableDump]
        int Count { get { return _count; } }
        [DisableDump]
        internal override bool IsDataLess { get { return false; } }

        void AssertValid() { Tracer.Assert(!_elementType.IsDataLess); }
        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }

        ISuffixFeature ISearchPath<ISuffixFeature, Array>.Convert(Array type)
        {
            if (type.ElementType != ElementType)
                return null;
            return Extension.Feature(type.ConvertToReference(_count));
        }
    }
}