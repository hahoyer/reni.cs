#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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
using hw.Debug;
using Reni.Basics;
using Reni.Code;
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
        , ISymbolInheritor
    {
        readonly TypeBase _valueType;
        readonly int _order;

        internal PointerType(TypeBase valueType)
        {
            _order = CodeArgs.NextOrder++;
            _valueType = valueType;
            Tracer.Assert(!valueType.IsDataLess, valueType.Dump);
            Tracer.Assert(!(valueType is PointerType), valueType.Dump);
            StopByObjectId(-10);
        }

        Size IContextReference.Size { get { return Size; } }
        int IContextReference.Order { get { return _order; } }
        IConverter IReferenceType.Converter { get { return this; } }
        bool IReferenceType.IsWeak { get { return true; } }

        [DisableDump]
        internal override Root RootContext { get { return _valueType.RootContext; } }

        IConverter IProxyType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return ValueType; } }
        Result IConverter.Result(Category category) { return DereferenceResult(category); }
        Result ISymbolInheritor.Source(Category category) { return DereferenceResult(category); }

        internal override string DumpPrintText { get { return GetNodeDump(); } }

        [DisableDump]
        TypeBase ValueType { get { return _valueType; } }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return ValueType.FindRecentStructure; } }

        [DisableDump]
        internal override IFeatureImplementation Feature { get { return ValueType.Feature; } }

        [DisableDump]
        internal override bool IsDataLess { get { return false; } }

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
                    , () => ArgCode.DePointer(ValueType.Size)
                    , CodeArgs.Arg
                );
        }

        protected override Size GetSize() { return Root.DefaultRefAlignParam.RefSize; }

        internal override ResultCache DePointer(Category category)
        {
            return ValueType
                .Result
                (category
                    , () => ArgCode.DePointer(ValueType.Size)
                    , CodeArgs.Arg
                );
        }

        protected override ArrayType ObtainArray(int count) { return ValueType.UniqueArray(count); }

        internal override IFeatureImplementation AlignConversion(TypeBase destination) { return destination == ValueType ? Extension.Feature(ArgResult) : null; }
    }
}