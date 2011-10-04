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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Feature;
using Reni.Sequence;
using Reni.Syntax;

namespace Reni.Type
{
    sealed class TextItemType : TagChild<TypeBase>, IResultProvider
    {
        [DisableDump]
        public readonly ISearchPath<ISuffixFeature, SequenceType> ToNumberOfBaseFeature;
        
        public TextItemType(TypeBase parent)
            : base(parent) { ToNumberOfBaseFeature = new ToNumberOfBaseFeature(this); }
        
        protected override string TagTitle { get { return "text_item"; } }

        internal override void Search(SearchVisitor searchVisitor)
        {
            Parent.Search(searchVisitor.Child(this));
            base.Search(searchVisitor);
        }

        internal override Result DumpPrintTextResultFromSequence(Category category, RefAlignParam refAlignParam, int count)
        {
            return Void.Result
                (category
                 , () => DumpPrintCodeFromSequence(refAlignParam, count)
                 , CodeArgs.Arg
                );
        }

        internal Result DumpPrintTextResult(Category category, RefAlignParam refAlignParam)
        {
            return Void.Result
                (category
                 , () => DumpPrintCode(refAlignParam)
                 , CodeArgs.Arg
                );
        }

        CodeBase DumpPrintCodeFromSequence(RefAlignParam refAlignParam, int count)
        {
            return
                UniqueSequence(count)
                    .UniqueAutomaticReference(refAlignParam)
                    .ArgCode()
                    .Dereference(refAlignParam, Size * count)
                    .DumpPrintText(Size);
        }

        CodeBase DumpPrintCode(RefAlignParam refAlignParam)
        {
            return
                UniqueAutomaticReference(refAlignParam)
                    .ArgCode()
                    .Dereference(refAlignParam, Size)
                    .DumpPrintText(Size);
        }
        protected override Result ChildConversionResult(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }
    }
}