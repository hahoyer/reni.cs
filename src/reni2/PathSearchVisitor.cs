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

namespace Reni
{
    sealed class PathSearchVisitor : SearchVisitor
    {
        [EnableDump]
        readonly FoundSearchVisitor _parent;
        [EnableDump]
        readonly IFoundItem _foundItem;
        internal PathSearchVisitor(FoundSearchVisitor parent, IFoundItem foundItem)
        {
            _parent = parent;
            _foundItem = foundItem;
        }
        bool IsSuccessFull { get { return _parent.IsSuccessFull; } }

        public override ISearchVisitor Path(IFoundItem foundItem)
        {
            NotImplementedMethod(foundItem);
            return null;
        }
        internal override void SearchTypeBase()
        {
            if(IsSuccessFull)
                return;
            _parent.SearchTypeBase();
            if(IsSuccessFull)
                _parent.Add(_foundItem);
        }

        internal override ISearchVisitor InternalChild<TType>(TType target) { return _parent.InternalChild(target).Path(_foundItem); }
    }
}