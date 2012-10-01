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

namespace Reni.Type
{
    abstract class TagChild<TParent> : Child<TParent>
        where TParent : TypeBase
    {
        protected TagChild(TParent parent)
            : base(parent) { }

        [DisableDump]
        protected abstract string TagTitle { get; }
        [DisableDump]
        internal override string DumpPrintText { get { return Parent.DumpPrintText + " #(# " + TagTitle + " #)#"; } }
        [DisableDump]
        internal override bool IsDataLess { get { return Parent.IsDataLess; } }
        protected override Size GetSize() { return Parent.Size; }
        protected override string GetNodeDump() { return Parent.NodeDump + "[" + TagTitle + "]"; }
        internal override Result Destructor(Category category) { return Parent.Destructor(category); }
        internal override Result ArrayDestructor(Category category, int count) { return Parent.ArrayDestructor(category, count); }
        internal override Result Copier(Category category) { return Parent.Copier(category); }
        internal override Result ArrayCopier(Category category, int count) { return Parent.ArrayCopier(category, count); }
        protected override Result ParentConversionResult(Category category) { return Parent.Result(category, ArgResult(category.Typed)); }
        internal Result ReferenceConversionResult(Category category) {return ReferenceConversionResult(category, Parent);
        }
    }
}