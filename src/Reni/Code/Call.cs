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

using Reni.Basics;
using Reni.Struct;

namespace Reni.Code
{
    sealed class Call : FiberItem
    {
        [Node]
        [DisableDump]
        internal readonly FunctionId FunctionId;

        [Node]
        [DisableDump]
        readonly Size ResultSize;

        [Node]
        [DisableDump]
        internal readonly Size ArgsAndRefsSize;

        internal Call(FunctionId functionId, Size resultSize, Size argsAndRefsSize)
        {
            FunctionId = functionId;
            ResultSize = resultSize;
            ArgsAndRefsSize = argsAndRefsSize;
            StopByObjectIds(-1);
        }

        [DisableDump]
        internal override Size InputSize => ArgsAndRefsSize;

        [DisableDump]
        internal override Size OutputSize => ResultSize;

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual) => actual.Call(this);

        protected override string GetNodeDump() => base.GetNodeDump() + " FunctionId=" + FunctionId + " ArgsAndRefsSize=" + ArgsAndRefsSize;

        internal override void Visit(IVisitor visitor) => visitor.Call(OutputSize, FunctionId, ArgsAndRefsSize);

        public FiberItem TryConvertToRecursiveCall(FunctionId functionId)
        {
            if(FunctionId != functionId)
                return this;
            ResultSize.IsZero.Assert();
            return ArgsAndRefsSize.GetRecursiveCall();
        }
    }
}