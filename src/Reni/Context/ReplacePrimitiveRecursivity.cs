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

using Reni.Code;
using Reni.Code.ReplaceVisitor;
using Reni.Struct;

namespace Reni.Context
{
    sealed class ReplacePrimitiveRecursivity : Base
    {
        static int NextObjectId;

        [EnableDump]
        readonly FunctionId FunctionId;

        public ReplacePrimitiveRecursivity(FunctionId functionId)
            : base(NextObjectId++) { FunctionId = functionId; }

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

        internal override FiberItem Call(Call visitedObject) => visitedObject.TryConvertToRecursiveCall(FunctionId);
    }
}