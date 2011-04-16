using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Code;
using Reni.Code.ReplaceVisitor;

namespace Reni.Context
{
    internal sealed class ReplacePrimitiveRecursivity : Base
    {
        private static int _nextObjectId;

        [IsDumpEnabled(true)]
        private readonly int _functionIndex;

        public ReplacePrimitiveRecursivity(int functionIndex)
            : base(_nextObjectId++) { _functionIndex = functionIndex; }

        public int FunctionIndex { get { return _functionIndex; } }

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