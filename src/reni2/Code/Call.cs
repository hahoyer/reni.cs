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
        [Node, IsDumpEnabled(false)]
        internal readonly int FunctionIndex;

        [Node, IsDumpEnabled(false)]
        private readonly Size _resultSize;

        [Node, IsDumpEnabled(false)]
        internal readonly Size ArgsAndRefsSize;

        internal Call(int functionIndex, Size resultSize, Size argsAndRefsSize)
        {
            FunctionIndex = functionIndex;
            _resultSize = resultSize;
            ArgsAndRefsSize = argsAndRefsSize;
        }

        [IsDumpEnabled(false)]
        internal override Size InputSize { get { return ArgsAndRefsSize; } }

        [IsDumpEnabled(false)]
        internal override Size OutputSize { get { return _resultSize; } }

        protected override FiberItem VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Call(this); }

        [IsDumpEnabled(false)]
        public override string NodeDump { get { return base.NodeDump + " FunctionIndex=" + FunctionIndex + " ArgsAndRefsSize=" + ArgsAndRefsSize; } }

        protected override string CSharpCodeSnippet(Size top) { return CSharpGenerator.Call(FunctionIndex); }

        protected override void Execute(IFormalMaschine formalMaschine) { formalMaschine.Call(OutputSize, FunctionIndex, ArgsAndRefsSize); }

        public FiberItem TryConvertToRecursiveCall(int functionIndex)
        {
            if(FunctionIndex != functionIndex)
                return this;
            Tracer.Assert(_resultSize.IsZero);
            return CodeBase.CreateRecursiveCall(ArgsAndRefsSize);
        }
    }
}