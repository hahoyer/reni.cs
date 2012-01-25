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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;

namespace Reni.Type
{
    abstract class ReferenceType : TypeBase
    {
        [DisableDump]
        internal abstract RefAlignParam RefAlignParam { get; }
        [DisableDump]
        internal abstract TypeBase ValueType { get; }
        [DisableDump]
        internal override int ArrayElementCount { get { return ValueType.ArrayElementCount; } }
        [DisableDump]
        internal override bool IsArray { get { return ValueType.IsArray; } }

        protected override Size GetSize() { return RefAlignParam.RefSize; }
        internal override int SequenceCount(TypeBase elementType) { return ValueType.SequenceCount(elementType); }
        internal override TypeBase SmartReference(RefAlignParam refAlignParam) { return this; }
        internal override TypeBase TypeForTypeOperator() { return ValueType.TypeForTypeOperator(); }

        protected abstract Result DereferenceResult(Category category);
        internal override Result AutomaticDereferenceResult(Category category) { return DereferenceResult(category).AutomaticDereference(); }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(ValueType, new ConversionFunction(this));
            base.Search(searchVisitor);
        }

        sealed class ConversionFunction : Reni.ConversionFunction
        {
            readonly ReferenceType _parent;
            public ConversionFunction(ReferenceType parent)
                : base(parent) { _parent = parent; }
            internal override Result Result(Category category)
            {
                var trace = ObjectId == -1 && category.HasCode;
                StartMethodDump(trace, category);
                try
                {
                    BreakExecution();
                    var result = _parent.ToAutomaticReferenceResult(category);
                    return ReturnMethodDump(result, true);
                }
                finally
                {
                    EndMethodDump();
                }
            }
        }

        internal override Result SmartLocalReferenceResult(Category category, RefAlignParam refAlignParam)
        {
            return UniqueAlign(refAlignParam.AlignBits)
                .Result
                (category
                 , () => LocalReferenceCode(refAlignParam).Dereference(refAlignParam, refAlignParam.RefSize)
                 , () => Destructor(Category.CodeArgs).CodeArgs + CodeArgs.Arg()
                );
        }

        internal override Result ReferenceInCode(Category category, IReferenceInCode target) { return ValueType.ReferenceInCode(category, target); }

        protected abstract Result ToAutomaticReferenceResult(Category category);

        Converter Converter(ConversionParameter conversionParameter, AutomaticReferenceType destination)
        {
            var trace = ObjectId == -13;
            try
            {
                StartMethodDump(trace, conversionParameter, destination);
                if(ValueType == destination.ValueType && destination.RefAlignParam == RefAlignParam)
                    return ReturnMethodDump(new FunctionalConverter(ToAutomaticReferenceResult), true);

                var c1 = new FunctionalConverter(DereferenceResult);
                Dump("c1", c1.Result(Category.Type | Category.Code));
                var c2 = ValueType.UnAlignedType.Converter(conversionParameter, destination.ValueType);
                Dump("c2", c2.Result(Category.Type | Category.Code));
                var c3 = new FunctionalConverter(destination.ValueTypeToLocalReferenceResult);
                Dump("c3", c3.Result(Category.Type | Category.Code));
                BreakExecution();
                var result = c1 * c2 * c3;
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }

        protected override Converter ConverterForDifferentTypes(ConversionParameter conversionParameter, TypeBase destination)
        {
            var referenceDestination = destination as AutomaticReferenceType;
            if(referenceDestination != null)
                return Converter(conversionParameter, referenceDestination);

            return
                DereferenceResult
                * ValueType.Converter(conversionParameter, destination);
        }
    }
}