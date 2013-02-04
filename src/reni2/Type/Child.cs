#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using System;
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;

namespace Reni.Type
{
    abstract class Child<TParent> : TypeBase, IProxyType, IConverter
        where TParent : TypeBase
    {
        readonly TParent _parent;

        protected Child(TParent parent) { _parent = parent; }

        [DisableDump]
        internal override sealed Root RootContext { get { return _parent.RootContext; } }
        [Node]
        [DisableDump]
        public TParent Parent { get { return _parent; } }

        IConverter IProxyType.Converter { get { return this; } }
        TypeBase IConverter.TargetType { get { return _parent; } }
        Result IConverter.Result(Category category) { return ParentConversionResult(category); }
        protected abstract Result ParentConversionResult(Category category);
    }
}