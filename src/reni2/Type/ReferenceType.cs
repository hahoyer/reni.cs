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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Sequence;

namespace Reni.Type
{
    internal abstract class ReferenceType : TypeBase, IReference
    {
        private readonly TypeBase _valueType;

        protected ReferenceType(TypeBase valueType) { _valueType = valueType; }

        [DisableDump]
        TypeBase IReference.ValueType { get { return ValueType; } }

        [DisableDump]
        RefAlignParam IReference.RefAlignParam { get { return RefAlignParam; } }

        [DisableDump]
        internal abstract RefAlignParam RefAlignParam { get; }

        virtual internal TypeBase ValueType { get { return _valueType; } }

        internal override int SequenceCount(TypeBase elementType) { return ValueType.SequenceCount(elementType); }
        internal override TypeBase ForceReference(RefAlignParam refAlignParam) { return this; }
        protected override Size GetSize() { return RefAlignParam.RefSize; }
        internal override bool IsRef(RefAlignParam refAlignParam) { return (RefAlignParam == refAlignParam); }
        internal override TypeBase TypeForTypeOperator() { return ValueType.TypeForTypeOperator(); }

        internal override bool VirtualIsConvertable(SequenceType destination, ConversionParameter conversionParameter)
        {
            return ValueType == destination
                   || ValueType.IsConvertable(destination, conversionParameter);
        }
        
        internal override bool VirtualIsConvertable(AutomaticReferenceType destination, ConversionParameter conversionParameter)
        {
            return ValueType == destination.ValueType
                   || ValueType.IsConvertable(destination.ValueType, conversionParameter);
        }

        internal override Result VirtualForceConversion(Category category, SequenceType destination)
        {
            if(ValueType == destination)
                return DereferenceResult(category);

            var typedCategory = category | Category.Type;
            return ValueType
                .ForceConversion(typedCategory, destination)
                .ReplaceArg(DereferenceResult(typedCategory));
        }

        internal override Result VirtualForceConversion(Category category, AutomaticReferenceType destination) { return ForceConversion(category, destination.ValueType).LocalReferenceResult(destination.RefAlignParam); }

        private Result DereferenceResult(Category category) { return ValueType.Result(category, DereferenceCode); }
        protected abstract CodeBase DereferenceCode();
        internal override Result AutomaticDereferenceResult(Category category) { return DereferenceResult(category).AutomaticDereference(); }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            ValueType.Search(searchVisitor);
            base.Search(searchVisitor);
        }

    }
}