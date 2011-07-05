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
using Reni.Sequence;

namespace Reni.Type
{
    internal sealed class Aligner : Child
    {
        private readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(-130);
        }

        [DisableDump]
        protected override bool IsInheritor { get { return true; } }

        [DisableDump]
        internal int AlignBits { get { return _alignBits; } }

        [DisableDump]
        internal override Size UnrefSize { get { return Parent.UnrefSize; } }

        [DisableDump]
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }

        internal override int SequenceCount(TypeBase elementType) { return Parent.SequenceCount(elementType); }
        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override TypeBase TypeForTypeOperator() { return Parent.TypeForTypeOperator(); }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }

        internal bool VirtualIsConvertable(TypeBase destination, ConversionParameter conversionParameter) { return Parent.IsConvertable(destination, conversionParameter); }
        internal override bool VirtualIsConvertable(SequenceType destination, ConversionParameter conversionParameter) { return Parent.IsConvertable(destination, conversionParameter); }
        protected override bool VirtualIsConvertableFrom(TypeBase source, ConversionParameter conversionParameter) { return source.VirtualIsConvertable(this, conversionParameter); }

        internal override bool HasConverterTo(TypeBase destination) { return Parent.HasConverterTo(destination); }
        internal override string DumpShort() { return base.DumpShort() + "(" + Parent.DumpShort() + ")"; }

        internal override AutomaticReferenceType SpawnAutomaticReference(RefAlignParam refAlignParam)
        {
            if(_alignBits == refAlignParam.AlignBits)
                return Parent.SpawnAutomaticReference(refAlignParam);
            return base.SpawnAutomaticReference(refAlignParam);
        }

        protected Result VirtualForceConversion(Category category, TypeBase destination)
        {
            return Parent
                .ForceConversion(category, destination)
                .ReplaceArg(CreateUnalignedArgResult(category));
        }

        private Result CreateUnalignedArgResult(Category category)
        {
            return Parent.Result
                (
                    category,
                    () => ArgCode().BitCast(Parent.Size)
                );
        }
    }
}