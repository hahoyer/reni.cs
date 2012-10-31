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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Struct;

namespace Reni.Type
{
    sealed class PointerType
        : TypeBase
          , IProxyType
          , IConverter
          , IReferenceType
    {
        readonly TypeBase _valueType;

        internal PointerType(TypeBase valueType)
        {
            _valueType = valueType;
            Tracer.Assert(!valueType.IsDataLess, valueType.Dump);
            Tracer.Assert(!(valueType is PointerType), valueType.Dump);
            StopByObjectId(-10);
        }

        IConverter IReferenceType.Converter { get { return this; } }
        bool IReferenceType.IsWeak { get { return true; } }
        [DisableDump]
        internal override Root RootContext { get { return _valueType.RootContext; } }
        IConverter IProxyType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return ValueType; } }
        Result IConverter.Result(Category category) { return DereferenceResult(category); }
        internal override string DumpPrintText { get { return GetNodeDump(); } }
        [DisableDump]
        TypeBase ValueType { get { return _valueType; } }
        [DisableDump]
        internal override Structure FindRecentStructure { get { return ValueType.FindRecentStructure; } }
        [DisableDump]
        internal override bool IsDataLess { get { return false; } }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return ValueType.TypeForTypeOperator; } }
        [DisableDump]
        internal override TypeBase CoreType { get { return ValueType.CoreType; } }

        protected override string GetNodeDump() { return ValueType.NodeDump + "[Pointer]"; }

        internal override int? SmartSequenceLength(TypeBase elementType)
        {
            return ValueType
                .SmartSequenceLength(elementType);
        }

        internal override int? SmartArrayLength(TypeBase elementType)
        {
            return ValueType
                .SmartArrayLength(elementType);
        }

        Result DereferenceResult(Category category)
        {
            return ValueType
                .Result
                (category
                 , () => ArgCode.Dereference(ValueType.Size)
                 , CodeArgs.Arg
                );
        }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => ValueType);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        protected override ArrayType ObtainArray(int count) { return ValueType.UniqueArray(count); }

        internal override ISuffixFeature AlignConversion(TypeBase destination)
        {
            if(destination != ValueType)
                return null;

            return Extension.Feature(ArgResult);
        }
    }
}