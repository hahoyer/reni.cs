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

using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.TreeStructure;

namespace Reni.Type
{
    [Serializable]
    internal abstract class Child<TParent> : TypeBase
        where TParent: TypeBase
    {
        private readonly TParent _parent;

        protected Child(TParent parent) { _parent = parent; }

        [Node]
        [DisableDump]
        public TParent Parent { get { return _parent; } }

        [DisableDump]
        protected internal override int IndexSize { get { return Parent.IndexSize; } }

        protected abstract bool IsInheritor { get; }

        internal override void Search(ISearchVisitor searchVisitor)
        {
            base.Search(searchVisitor);
            if (IsInheritor)
                Parent.Search(searchVisitor);
        }
    }
}