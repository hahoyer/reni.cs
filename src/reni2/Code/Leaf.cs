using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    /// Container of a leaf
    /// </summary>
    [Serializable]
    internal sealed class Leaf : CodeBase
    {
        private readonly LeafElement _leafElement;

        // Warnind disabled, since it is used for debugging purposes
        [UsedImplicitly]
        internal static bool TryToCombine;

        internal Leaf(LeafElement leafElement)
        {
            _leafElement = leafElement;
            StopByObjectId(-526);
        }

        [DumpData(false)]
        protected override Size SizeImplementation { get { return LeafElement.Size; } }

        [Node]
        internal LeafElement LeafElement { get { return _leafElement; } }

        [DumpData(false)]
        internal override bool IsEmpty { get { return LeafElement.IsEmpty; } }

        [DumpData(false)]
        internal override RefAlignParam RefAlignParam { get { return LeafElement.RefAlignParam; } }

        internal override CodeBase CreateChild(LeafElement leafElement)
        {
            if(!TryToCombine)
                return base.CreateChild(leafElement);

            var newLeafElements = LeafElement.TryToCombineN(leafElement);
            if(newLeafElements == null)
                return base.CreateChild(leafElement);

            CodeBase result = new Leaf(newLeafElements[0]);
            for(var i = 1; i < newLeafElements.Length; i++)
                result = result.CreateChild(newLeafElements[i]);
            return result;
        }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual) { return actual.Leaf(LeafElement); }

        internal override BitsConst Evaluate() { return LeafElement.Evaluate(); }
    }
}