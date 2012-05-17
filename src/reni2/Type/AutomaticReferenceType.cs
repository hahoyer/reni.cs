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
using Reni.Code;
using Reni.Struct;

namespace Reni.Type
{
    sealed class AutomaticReferenceType : TypeBase, IContainerType, IConverter
    {
        readonly TypeBase _valueType;
        readonly RefAlignParam _refAlignParam;

        internal AutomaticReferenceType(TypeBase valueType, RefAlignParam refAlignParam)
        {
            _valueType = valueType;
            _refAlignParam = refAlignParam;
            Tracer.Assert(!valueType.IsDataLess, valueType.Dump);
            Tracer.Assert(!(valueType is AutomaticReferenceType), valueType.Dump);
            StopByObjectId(-14);
        }


        [DisableDump]
        internal override RefAlignParam[] ReferenceChain
        {
            get
            {
                var subResult = ValueType.ReferenceChain;
                var result = new RefAlignParam[subResult.Length + 1];
                result[0] = RefAlignParam;
                subResult.CopyTo(result, 1);
                return result;
            }
        }

        internal override string DumpPrintText { get { return DumpShort(); } }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return _refAlignParam; } }
        internal TypeBase ValueType { get { return _valueType; } }
        [DisableDump]
        internal override bool IsLikeReference { get { return true; } }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return ValueType.FindRecentStructure; } }

        Result DereferenceResult(Category category)
        {
            return ValueType.Result
                (category
                 , () => ArgCode().Dereference(RefAlignParam, ValueType.Size)
                 , CodeArgs.Arg
                );
        }

        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        internal override int ArrayElementCount { get { return ValueType.ArrayElementCount; } }
        [DisableDump]
        internal override bool IsArray { get { return ValueType.IsArray; } }
        TypeBase IContainerType.Target { get { return ValueType; } }

        IConverter IContainerType.Converter() { return this; }
        Result IConverter.Result(Category category) { return DereferenceResult(category); }

        Result ToAutomaticReferenceResult(Category category) { return ArgResult(category); }

        Result ValueTypeToLocalReferenceResult(Category category) { return ValueType.SmartLocalReferenceResult(category, RefAlignParam); }
        protected override Size GetSize() { return RefAlignParam.RefSize; }
        internal override int SequenceCount(TypeBase elementType) { return ValueType.SequenceCount(elementType); }
        internal override TypeBase SmartReference(RefAlignParam refAlignParam) { return this; }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator{get { return ValueType.TypeForTypeOperator; }}
        internal override Result AutomaticDereferenceResult(Category category) { return DereferenceResult(category).AutomaticDereference(); }
        
        internal override void Search(SearchVisitor searchVisitor)
        {
            ValueType.Search(searchVisitor.Child(this));
            searchVisitor.SearchAndConvert(ValueType, this);
            base.Search(searchVisitor);
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
        internal override Result ReferenceInCode(Category category, IReferenceInCode target)
        {
            return ValueType
                .ReferenceInCode(category, target);
        }
        IConverter Converter(ConversionParameter conversionParameter, AutomaticReferenceType destination)
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
                var result = c1.Concat(c2).Concat(c3);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
        protected override IConverter ConverterForDifferentTypes(ConversionParameter conversionParameter, TypeBase destination)
        {
            var referenceDestination = destination as AutomaticReferenceType;
            if(referenceDestination != null)
                return Converter(conversionParameter, referenceDestination);

            return
                new FunctionalConverter(DereferenceResult)
                    .Concat(ValueType.Converter(conversionParameter, destination));
        }

    }
}