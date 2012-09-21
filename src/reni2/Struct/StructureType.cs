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
using Reni.Feature.DumpPrint;
using Reni.ReniParser;
using Reni.Type;

namespace Reni.Struct
{
    sealed class StructureType : TypeBase
    {
        readonly Structure _structure;

        [DisableDump]
        internal readonly ISearchPath<ISuffixFeature, PointerType> DumpPrintReferenceFeature;

        internal StructureType(Structure structure)
        {
            _structure = structure;
            DumpPrintReferenceFeature = new StructReferenceFeature(this);
        }

        [DisableDump]
        internal RefAlignParam RefAlignParam { get { return Structure.RefAlignParam; } }

        [DisableDump]
        internal Structure Structure { get { return _structure; } }

        internal override Root RootContext { get { return _structure.RootContext; } }
        protected override Size GetSize() { return Structure.StructSize; }

        internal override string DumpShort() { return "type(" + Structure.DumpShort() + ")"; }

        internal override void Search(SearchVisitor searchVisitor, ExpressionSyntax syntax)
        {
            searchVisitor.Search(this);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor, syntax);
        }

        internal void SearchNameSpace<TFeature>(SearchVisitor<TFeature> searchVisitor)
            where TFeature : class, ISearchPath
        {
            var searchVisitorChild = searchVisitor as SearchVisitor<ISuffixFeature>;
            if(searchVisitorChild == null || searchVisitorChild.IsSuccessFull)
                return;
            searchVisitorChild.InternalResult = Structure
                .Search(searchVisitorChild.Target)
                .CheckedConvert(this);
        }

        [DisableDump]
        internal override Structure FindRecentStructure { get { return Structure; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Structure.IsDataLess; } }

        internal Result DumpPrintResult(Category category) { return Structure.DumpPrintResultViaStructReference(category); }

        [DisableDump]
        internal override bool HasQuickSize { get { return false; } }
    }
}