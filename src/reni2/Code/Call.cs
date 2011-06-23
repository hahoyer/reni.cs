using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using Reni.Context;

namespace Reni.Code
{
    [Serializable]
    internal sealed class Call : FiberItem
    {
        [Node, DisableDump]
        internal readonly int FunctionIndex;

        [Node, DisableDump]
        private readonly Size _resultSize;

        [Node, DisableDump]
        internal readonly Size ArgsAndRefsSize;

        internal Call(int functionIndex, Size resultSize, Size argsAndRefsSize)
        {
            FunctionIndex = functionIndex;
            _resultSize = resultSize;
            ArgsAndRefsSize = argsAndRefsSize;
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