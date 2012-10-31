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
        : RepeaterType
          , IProxyType
          , IConverter
          , ISearchPath<ISuffixFeature, ArrayType>
          , ISearchPath<ISuffixFeature, RepeaterAccessType>

    {
        internal ReferenceType(TypeBase elementType, int count)
            : base(elementType, count) { StopByObjectId(-22); }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        internal override string DumpPrintText { get { return "(" + ElementType.DumpPrintText + ")reference(" + Count + ")"; } }

        internal Result EnableRawConversion(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }

        ISuffixFeature ISearchPath<ISuffixFeature, ArrayType>.Convert(ArrayType type)
        {
            if(type.ElementType != ElementType)
                return null;
            return Extension.Feature(type.ConvertToReference(Count));
        }
        
        ISuffixFeature ISearchPath<ISuffixFeature, RepeaterAccessType>.Convert(RepeaterAccessType type)
        {
            if (type.RepeaterType.ElementType != ElementType)
                return null;
            return Extension.Feature(type.ConvertToReference(Count));
        }

        IConverter IProxyType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return ElementType; } }

        Result IConverter.Result(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}