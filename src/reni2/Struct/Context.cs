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
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;

namespace Reni.Struct
{
    sealed class Context : Child
    {
        [Node]
        internal readonly int Position;
        [Node]
        internal readonly Container Container;

        internal Context(ContextBase parent, Container container, int position)
            : base(parent)
        {
            Position = position;
            Container = container;
        }

        [DisableDump]
        internal Structure Structure { get { return Parent.UniqueStructure(Container, Position); } }

        internal override void Search(ContextSearchVisitor searchVisitor)
        {
            searchVisitor.Search(this);
            if(searchVisitor.IsSuccessFull)
                return;
            base.Search(searchVisitor);
        }

        internal Result ObjectResult(Category category) { return Structure.StructReferenceViaContextReference(category); }

        internal override Structure ObtainRecentStructure() { return Structure; }
    }
}