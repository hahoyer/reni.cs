#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    sealed class TextItemsType : TagChild<ArrayType>
    {
        [DisableDump]
        public readonly ISuffixFeature ToNumberOfBaseFeature;

        public TextItemsType(ArrayType parent)
            : base(parent) { ToNumberOfBaseFeature = new ToNumberOfBaseFeature(this); }

        [DisableDump]
        protected override string TagTitle { get { return "text_items"; } }
        [DisableDump]
        internal override IFeature Feature { get { return Parent.Feature; } }

        internal override void Search(SearchVisitor searchVisitor)
        {
            searchVisitor.Search(this, () => Parent);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor);
        }

        internal override int? SmartArrayLength(TypeBase elementType) { return Parent.SmartArrayLength(elementType); }

        internal Result ConcatArrays(Category category, IContextReference objectReference, TypeBase argsType)
        {
            var trace = ObjectId == -1 && category.HasCode;
            StartMethodDump(trace, category, objectReference, argsType);
            try
            {
                var result = Parent.InternalConcatArrays(category.Typed, objectReference, argsType);
                Dump("result", result);
                BreakExecution();

                var type = (ArrayType) result.Type;
                return ReturnMethodDump(type.UniqueTextItemsType.Result(category, result));
            }
            finally
            {
                EndMethodDump();
            }
        }

        internal Result DumpPrintTextResult(Category category)
        {
            return VoidType
                .Result
                (category
                 , DumpPrintCode
                 , CodeArgs.Arg
                );
        }

        CodeBase DumpPrintCode()
        {
            return UniquePointer
                .ArgCode
                .DePointer(Size)
                .DumpPrintText(Parent.ElementType.Size);
        }
    }
}