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
using Reni.Basics;
using Reni.Code;
using Reni.Struct;

namespace Reni.Type
{
    internal sealed class AutomaticReferenceType : ReferenceType
    {
        private readonly RefAlignParam _refAlignParam;

        internal AutomaticReferenceType(TypeBase valueType, RefAlignParam refAlignParam)
            : base(valueType)
        {
            _refAlignParam = refAlignParam;
            //Tracer.Assert(!valueType.IsZeroSized, valueType.Dump);
            Tracer.Assert(!(valueType is AutomaticReferenceType), valueType.Dump);
            StopByObjectId(-8);
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
        internal override RefAlignParam RefAlignParam { get { return _refAlignParam; } }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return ValueType.FindRecentStructure; } }

        protected override CodeBase DereferenceCode() { return ArgCode().Dereference(RefAlignParam, ValueType.Size); }

        internal override Result ReferenceInCode(IReferenceInCode target, Category category)
        {
            return Result
                (
                    category,
                    () => CodeBase.ReferenceCode(target).Dereference(target.RefAlignParam, target.RefAlignParam.RefSize),
                    () => Refs.Create(target)
                );
        }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            ValueType.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        internal override Result ToAutomaticReferenceResult(Category category) { return ArgResult(category); }

        internal static Converter Converter(ReferenceType source, ConversionParameter conversionParameter, AutomaticReferenceType destination)
        {
            if(source.ValueType == destination.ValueType && destination.RefAlignParam == source.RefAlignParam)
                return new FunctionalConverter(source.ToAutomaticReferenceResult);
            return
                source.DereferenceResult
                *source.ValueType.Converter(conversionParameter, destination.ValueType)
                *destination.ValueTypeToLocalReferenceResult;
        }

        private Result ValueTypeToLocalReferenceResult(Category category)
        {
            return ValueType
                .LocalReferenceResult(category, RefAlignParam);
        }
    }
}