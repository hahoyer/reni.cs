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
        internal override ISmartReference Reference { get { return Parent.Reference; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Parent.IsDataLess; } }
        [DisableDump]
        internal override ISmartReference UniqueSmartReference { get { return Parent.UniqueSmartReference; } }
        [DisableDump]
        internal override TypeBase TypeForTypeOperator { get { return Parent.TypeForTypeOperator; } }

        protected override Size GetSize() { return Parent.Size.Align(AlignBits); }
        internal override int? SmartSequenceLength(TypeBase elementType) { return Parent.SmartSequenceLength(elementType); }
        internal override int? SmartArrayLength(TypeBase elementType) { return Parent.SmartArrayLength(elementType); }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override Result ApplyTypeOperator(Result argResult) { return Parent.ApplyTypeOperator(argResult); }
        internal override string DumpShort() { return base.DumpShort() + "(" + Parent.DumpShort() + ")"; }
        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => Parent);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal override Result ParentConversionResult(Category category)
        {
            return Parent.Result
                (
                    category,
                    () => ArgCode.BitCast(Parent.Size)
                    , CodeArgs.Arg
                );
        }

        ISuffixFeature ISearchPath<ISuffixFeature, TypeBase>.Convert(TypeBase type) { return type.AlignConversion(Parent); }

        internal ISuffixFeature UnAlignConversion(TypeBase destination)
        {
            var childConverter = Parent.Converter(destination);
            if(childConverter != null)
                return new UnAlignConverter(this, childConverter);
            return null;
        }

        sealed class UnAlignConverter : ConverterBase
        {
            [EnableDump]
            readonly Aligner _sourceType;
            [EnableDump]
            readonly SearchResult _childConverter;
            public UnAlignConverter(Aligner sourceType, SearchResult childConverter)
            {
                _sourceType = sourceType;
                _childConverter = childConverter;
            }

            protected override Result Result(Category category)
            {
                return _childConverter
                    .Result(category)
                    .ReplaceArg(_sourceType.ParentConversionResult);
            }
        }
    }
}