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
    internal sealed class Aligner : Child
    {
        private readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(-6);
        }

        [DisableDump]
        protected override bool IsInheritor { get { return true; } }

        [DisableDump]
        private int AlignBits { get { return _alignBits; } }

        [DisableDump]
        internal override Size UnrefSize { get { return Parent.UnrefSize; } }

        [DisableDump]
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }
        [DisableDump]
        internal override TypeBase UnAlignedType { get { return Parent; } }

        internal override int SequenceCount(TypeBase elementType) { return Parent.SequenceCount(elementType); }
        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override TypeBase TypeForTypeOperator() { return Parent.TypeForTypeOperator(); }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }
        internal override string DumpShort() { return base.DumpShort() + "(" + Parent.DumpShort() + ")"; }

        internal override AutomaticReferenceType UniqueAutomaticReference(RefAlignParam refAlignParam)
        {
            if(_alignBits == refAlignParam.AlignBits)
                return Parent.UniqueAutomaticReference(refAlignParam);
            return base.UniqueAutomaticReference(refAlignParam);
        }

        internal Result UnalignedResult(Category category)
        {
            return Parent.Result
                (
                    category,
                    () => ArgCode().BitCast(Parent.Size)
                );
        }

        internal Result ParentToAlignedResult(Category c) { return Parent.ArgResult(c).Align(AlignBits); }
    }
}