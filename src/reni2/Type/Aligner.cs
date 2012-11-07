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
using Reni.Feature;

namespace Reni.Type
{
    sealed class Aligner
        : Child<TypeBase>
          , ISearchPath<ISuffixFeature, TypeBase>

    {
        readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(-9);
        }

        [DisableDump]
        internal int AlignBits { get { return _alignBits; } }
        [DisableDump]
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }

        [DisableDump]
        internal override IReferenceType ReferenceType { get { return Parent.ReferenceType; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Parent.IsDataLess; } }
        [DisableDump]
        internal override IReferenceType UniquePointerType { get { return Parent.UniquePointerType; } }

        internal override Result DeAlign(Category category) { return Parent.Result(category, ArgResult); }

        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }
        internal override int? SmartSequenceLength(TypeBase elementType) { return Parent.SmartSequenceLength(elementType); }
        internal override int? SmartArrayLength(TypeBase elementType) { return Parent.SmartArrayLength(elementType); }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }
        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + Parent.NodeDump + ")"; }
        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => Parent);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        protected override Result ParentConversionResult(Category category) { return Parent.UniquePointer.ArgResult(category); }

        ISuffixFeature ISearchPath<ISuffixFeature, TypeBase>.Convert(TypeBase type) { return type.AlignConversion(Parent); }

        public Result UnalignedResult(Category category) { return Parent.Result(category, () => ArgCode.BitCast(Parent.Size)); }
    }
}