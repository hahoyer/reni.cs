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
using Reni.Code;
using Reni.ReniParser;

namespace Reni.Type
{
    sealed class TextItemType : TagChild<TypeBase>
    {
        public TextItemType(TypeBase parent)
            : base(parent)
        {
            Tracer.Assert(!(parent is ArrayType));
            StopByObjectId(-10);
        }

        [DisableDump]
        protected override string TagTitle { get { return "text_item"; } }

        internal override void Search(SearchVisitor searchVisitor, ExpressionSyntax syntax)
        {
            searchVisitor.Search(this, () => Parent);
            if(!searchVisitor.IsSuccessFull)
                base.Search(searchVisitor, syntax);
        }

        internal Result DumpPrintTextResult(Category category)
        {
            return VoidType.Result
                (category
                 , DumpPrintCode
                 , CodeArgs.Arg
                );
        }

        CodeBase DumpPrintCode()
        {
            return UniquePointer
                .ArgCode
                .Dereference(Size)
                .DumpPrintText(Size);
        }
    }
}