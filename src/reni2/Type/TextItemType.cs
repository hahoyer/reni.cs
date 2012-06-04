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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Sequence;

namespace Reni.Type
{
    sealed class TextItemType : TagChild<TypeBase>
    {
        [DisableDump]
        public readonly ISearchPath<ISuffixFeature, SequenceType> ToNumberOfBaseFeature;

        public TextItemType(TypeBase parent)
            : base(parent) { ToNumberOfBaseFeature = new ToNumberOfBaseFeature(this); }

        protected override string TagTitle { get { return "text_item"; } }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, ()=>Parent);
            base.Search(searchVisitor);
        }

        protected override Result ParentConversionResult(Category category)
        {
            NotImplementedMethod(category);
            return null;

        }

        internal override Result DumpPrintTextResultFromSequence(Category category, int count)
        {
            return Void.Result
                (category
                 , () => DumpPrintCodeFromSequence(count)
                 , CodeArgs.Arg
                );
        }

        internal Result DumpPrintTextResult(Category category)
        {
            return Void.Result
                (category
                 , DumpPrintCode
                 , CodeArgs.Arg
                );
        }

        CodeBase DumpPrintCodeFromSequence(int count)
        {
            return
                UniqueSequence(count).UniqueReference.Type().ArgCode
                    .Dereference(Size * count)
                    .DumpPrintText(Size);
        }

        CodeBase DumpPrintCode()
        {
            return
                UniqueReference.Type().ArgCode
                    .Dereference(Size)
                    .DumpPrintText(Size);
        }
    }
}