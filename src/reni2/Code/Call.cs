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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    [Serializable]
    internal sealed class Call : FiberItem
    {
        [Node]
        [DisableDump]
        internal readonly int FunctionIndex;

        [Node]
        [DisableDump]
        private readonly Size _resultSize;

        [Node]
        [DisableDump]
        internal readonly Size ArgsAndRefsSize;

        internal Call(int functionIndex, Size resultSize, Size argsAndRefsSize)
        {
            FunctionIndex = functionIndex;
            _resultSize = resultSize;
            ArgsAndRefsSize = argsAndRefsSize;
            StopByObjectId(-2);
        }

        [DisableDump]
        internal override Size InputSize { get { return ArgsAndRefsSize; } }

        [DisableDump]
        internal override Size OutputSize { get { return _resultSize; } }

        protected override FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Call(this); }

        [DisableDump]
        public override string NodeDump { get { return base.NodeDump + " FunctionIndex=" + FunctionIndex + " ArgsAndRefsSize=" + ArgsAndRefsSize; } }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.Call(FunctionIndex); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Call(OutputSize, FunctionIndex, ArgsAndRefsSize); }

        public FiberItem TryConvertToRecursiveCall(int functionIndex)
        {
            if(FunctionIndex != functionIndex)
                return this;
            Tracer.Assert(_resultSize.IsZero);
            return CodeBase.RecursiveCall(ArgsAndRefsSize);
        }
    }
}