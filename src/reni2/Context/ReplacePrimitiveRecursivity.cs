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
using Reni.Code;
using Reni.Code.ReplaceVisitor;

namespace Reni.Context
{
    sealed class ReplacePrimitiveRecursivity : Base
    {
        static int _nextObjectId;

        [EnableDump]
        readonly int _functionIndex;

        public ReplacePrimitiveRecursivity(int functionIndex)
            : base(_nextObjectId++) { _functionIndex = functionIndex; }

        internal override CodeBase List(List visitedObject)
        {
            var visitor = this;
            var data = visitedObject.Data;
            var newList = new CodeBase[data.Length];
            var index = data.Length - 1;
            var codeBase = data[index];
            newList[index] = codeBase.Visit(visitor);
            return visitor.List(visitedObject, newList);
        }

        internal override CodeBase Fiber(Fiber visitedObject)
        {
            var data = visitedObject.FiberItems;
            var newItems = new FiberItem[data.Length];
            var index = data.Length - 1;
            newItems[index] = data[index].Visit(this);
            return Fiber(visitedObject, null, newItems);
        }

        internal override FiberItem Call(Call visitedObject) { return visitedObject.TryConvertToRecursiveCall(_functionIndex); }
    }
}