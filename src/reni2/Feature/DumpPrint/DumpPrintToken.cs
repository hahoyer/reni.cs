﻿#region Copyright (C) 2012

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
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    sealed class DumpPrintToken :
        Defineable,
        ISearchPath<ISearchPath<ISuffixFeature, AutomaticReferenceType>, StructureType>,
        ISearchPath<ISuffixFeature, TypeType>,
        ISearchPath<ISuffixFeature, Bit>,
        ISearchPath<ISuffixFeature, Type.Void>,
        ISearchPath<ISuffixFeature, StructureType>,
        ISearchPath<ISuffixFeature, Type.Array>,
        ISearchPath<ISuffixFeature, TextItemType>,
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Bit>,
        ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, TextItemType>
    {
        static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        static readonly BitFeature _bitFeature = new BitFeature();
        static readonly ISearchPath<ISuffixFeature, SequenceType> _dumpPrintSequenceFeature = new DumpPrintSequenceFeature();

        ISuffixFeature ISearchPath<ISuffixFeature, TypeType>.Convert(TypeType type) { return Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        ISuffixFeature ISearchPath<ISuffixFeature, Type.Void>.Convert(Type.Void type) { return Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, StructureType>.Convert(StructureType type) { return Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, Type.Array>.Convert(Type.Array type) { return Feature(type.DumpPrintResult); }
        ISuffixFeature ISearchPath<ISuffixFeature, TextItemType>.Convert(TextItemType type) { return Feature(type.DumpPrintTextResult); }

        ISearchPath<ISuffixFeature, SequenceType> ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, TextItemType>.Convert(TextItemType type) { return _dumpPrintSequenceFeature; }
        ISearchPath<ISuffixFeature, SequenceType> ISearchPath<ISearchPath<ISuffixFeature, SequenceType>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<ISuffixFeature, AutomaticReferenceType> ISearchPath<ISearchPath<ISuffixFeature, AutomaticReferenceType>, StructureType>.Convert(StructureType type) { return type.DumpPrintReferenceFeature; }

        internal static DumpPrintToken Create() { return new DumpPrintToken {Name = "<dump_print>"}; }
    }
}