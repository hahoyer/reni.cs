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
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    sealed class DumpPrintToken :
        Defineable,
        ISearchPath<ISuffixFeature, TypeType>,
        ISearchPath<ISuffixFeature, FunctionBodyType>,
        ISearchPath<ISuffixFeature, BitType>,
        ISearchPath<ISuffixFeature, VoidType>,
        ISearchPath<ISuffixFeature, StructureType>,
        ISearchPath<ISuffixFeature, ArrayType>,
        ISearchPath<ISuffixFeature, TextItemsType>,
        ISearchPath<ISuffixFeature, TextItemType>,
        ISearchPath<ISearchPath<ISuffixFeature, PointerType>, StructureType>,
        ISearchPath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, BitType>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, TypeType>.Convert(TypeType type) { return Extension.Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, BitType>.Convert(BitType type) { return Extension.Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, VoidType>.Convert(VoidType type) { return Extension.Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, StructureType>.Convert(StructureType type) { return Extension.Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, ArrayType>.Convert(ArrayType type) { return Extension.Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, TextItemType>.Convert(TextItemType type) { return Extension.Feature(type.DumpPrintTextResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, FunctionBodyType>.Convert(FunctionBodyType type) { return Extension.Feature(type.DumpPrintTextResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, TextItemsType>.Convert(TextItemsType type) { return Extension.Feature(type.DumpPrintTextResult); }

        ISearchPath<ISuffixFeature, PointerType> ISearchPath<ISearchPath<ISuffixFeature, PointerType>, StructureType>.Convert(StructureType type) { return type.DumpPrintReferenceFeature; }
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType> ISearchPath<ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, ArrayType>, BitType>.Convert(BitType type) { return Extension.Feature<SequenceType, ArrayType> (type.DumpPrintResult); }

        internal static DumpPrintToken Create() { return new DumpPrintToken {Name = "<dump_print>"}; }
    }
}