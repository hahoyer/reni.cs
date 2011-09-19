﻿//     Compiler for programming language "Reni"
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
using Reni.Feature;
using Reni.Feature.DumpPrint;
using Reni.Type;

namespace Reni.Struct
{
    sealed class StructureType : TypeBase
    {
        readonly Structure _structure;

        [DisableDump]
        internal readonly ISearchPath<ISuffixFeature, AutomaticReferenceType> DumpPrintReferenceFeature;

        internal StructureType(Structure structure)
        {
            _structure = structure;
            DumpPrintReferenceFeature = new StructReferenceFeature(this);
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return Structure.RefAlignParam; } }

        [DisableDump]
        internal ContainerContextObject ContainerContextObject { get { return Structure.ContainerContextObject; } }

        [DisableDump]
        internal Structure Structure { get { return _structure; } }

        internal override Size GetSize(bool isQuick) { return Structure.StructSize; }

        internal override string DumpShort() { return "type(" + Structure.DumpShort() + ")"; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<ISuffixFeature>;
            if(searchVisitorChild != null && !searchVisitorChild.IsSuccessFull)
                searchVisitorChild.InternalResult = Structure
                    .SearchFromRefToStruct(searchVisitorChild.Defineable)
                    .CheckedConvert(this);
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }
        Result SearchConverter(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
        internal override bool? IsDereferencedDataLess(bool isQuick) { return Structure.StructIsDataLess(isQuick); }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return Structure; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Structure.StructIsDataLess(false) == true; } }

        internal Result DumpPrintResult(Category category, RefAlignParam refAlignParam) { return Structure.DumpPrintResultViaStructReference(category); }
    }
}