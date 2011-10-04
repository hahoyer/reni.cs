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
using Reni.Basics;

namespace Reni.Type
{
    [Serializable]
    abstract class Child<TParent> : TypeBase
        where TParent : TypeBase
    {
        readonly TParent _parent;

        protected Child(TParent parent) { _parent = parent; }

        [Node]
        [DisableDump]
        public TParent Parent { get { return _parent; } }

        [DisableDump]
        protected internal override int IndexSize { get { return Parent.IndexSize; } }

        protected abstract bool IsInheritor { get; }

        internal override void Search(SearchVisitor searchVisitor)
        {
            base.Search(searchVisitor);
            if(IsInheritor)
                Parent.Search(searchVisitor, new ConversionFunction(this));
        }

        sealed class ConversionFunction : Reni.ConversionFunction
        {
            readonly Child<TParent> _parent;
            public ConversionFunction(Child<TParent> parent) { _parent = parent; }
            internal override Result Result(Category category) { return _parent.ChildConversionResult(category); }
            internal override TypeBase ArgType { get { return _parent; } }
        }

        protected abstract Result ChildConversionResult(Category category);
    }
}