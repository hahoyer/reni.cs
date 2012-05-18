#region Copyright (C) 2012

// 
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

namespace Reni.Type
{
    sealed class Aligner : Child<TypeBase>
    {
        readonly int _alignBits;

        public Aligner(TypeBase target, int alignBits)
            : base(target)
        {
            _alignBits = alignBits;
            StopByObjectId(-6);
        }

        [DisableDump]
        int AlignBits { get { return _alignBits; } }

        [DisableDump]
        internal override Size UnrefSize { get { return Parent.UnrefSize; } }

        [DisableDump]
        internal override string DumpPrintText { get { return "#(#align" + _alignBits + "#)# " + Parent.DumpPrintText; } }
        [DisableDump]
        internal override TypeBase UnAlignedType { get { return Parent; } }
        [DisableDump]
        internal override IReference Reference { get { return Parent.Reference; } }

        protected override Result ParentConversionResult(Category category)
        {
            return Parent.Result
                (
                    category,
                    () => ArgCode().BitCast(Parent.Size)
                    , CodeArgs.Arg
                );
        }

        internal override int SequenceCount(TypeBase elementType) { return Parent.SequenceCount(elementType); }

        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }
        internal override bool IsDataLess { get { return Parent.IsDataLess; } }

        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return Parent.TypeForTypeOperator; } }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }
        internal override string DumpShort() { return base.DumpShort() + "(" + Parent.DumpShort() + ")"; }

        internal Result ParentToAlignedResult(Category c) { return Parent.ArgResult(c).Align(AlignBits); }

        protected override IConverter ConverterForDifferentTypes(ConversionParameter conversionParameter, TypeBase destination)
        {
            return new FunctionalConverter(ParentConversionResult)
                .Concat(Parent.Converter(conversionParameter, destination));
        }
    }
}