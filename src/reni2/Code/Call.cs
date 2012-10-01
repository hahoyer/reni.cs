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
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Struct;

namespace Reni.Code
{
    [Serializable]
    sealed class Call : FiberItem
    {
        [Node]
        [DisableDump]
        internal readonly FunctionId FunctionId;

        [Node]
        [DisableDump]
        readonly Size _resultSize;

        [Node]
        [DisableDump]
        internal readonly Size ArgsAndRefsSize;

        internal Call(FunctionId functionId, Size resultSize, Size argsAndRefsSize)
        {
            FunctionId = functionId;
            _resultSize = resultSize;
            ArgsAndRefsSize = argsAndRefsSize;
            StopByObjectId(-1);
        }

        [DisableDump]
        internal override Size InputSize { get { return ArgsAndRefsSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _resultSize; } }

        protected override FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Call(this); }

        protected override string GetNodeDump() { return base.GetNodeDump() + " FunctionId=" + FunctionId + " ArgsAndRefsSize=" + ArgsAndRefsSize; }

        internal override void Visit(IVisitor visitor) { visitor.Call(OutputSize, FunctionId, ArgsAndRefsSize); }

        public FiberItem TryConvertToRecursiveCall(FunctionId functionId)
        {
            if(FunctionId != functionId)
                return this;
            Tracer.Assert(_resultSize.IsZero);
            return CodeBase.RecursiveCall(ArgsAndRefsSize);
        }
    }
}