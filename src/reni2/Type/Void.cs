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

namespace Reni.Type
{
    [Serializable]
    internal sealed class Void : TypeBase
    {
        internal override void Search(ISearchVisitor searchVisitor)
        {
            searchVisitor.ChildSearch(this);
            base.Search(searchVisitor);
        }

        internal override TypeBase ForceReference(RefAlignParam refAlignParam) { return this; }

        protected override TypeBase ReversePair(TypeBase first) { return first; }
        protected override Size GetSize() { return Size.Zero; }

        internal override TypeBase Pair(TypeBase second) { return second; }
        internal override string DumpPrintText { get { return "void"; } }
        internal override string DumpShort() { return "void"; }
        [DisableDump]
        internal override bool IsVoid { get { return true; } }
        internal Result DumpPrintResult(Category category, RefAlignParam refAlignParam) { return Result(category); }

        protected override bool VirtualIsConvertableFrom(TypeBase source, ConversionParameter conversionParameter) { return source.IsZeroSized; }
        protected override Result VirtualForceConversionFrom(Category category, TypeBase source) { return Result(category, source.ArgResult(category)); }
    }
}