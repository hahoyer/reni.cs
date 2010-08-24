using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using HWClassLibrary.TreeStructure;
using JetBrains.Annotations;
using Reni.Context;

#pragma warning disable 649

namespace Reni.Code
{
    /// <summary>
    /// Container of a leaf
    /// </summary>
    [Serializable]
    internal sealed class Leaf : CodeBase
    {
        private readonly StartingLeafElement _leafElement;

        // Warnind disabled, since it is used for debugging purposes
        [UsedImplicitly]
        internal static bool TryToCombine;

        internal Leaf(StartingLeafElement leafElement)
        {
            _leafElement = leafElement;
            StopByObjectId(-526);
        }

        [IsDumpEnabled(false)]
        protected override Size SizeImplementation { get { return LeafElement.Size; } }

        [Node]
        internal StartingLeafElement LeafElement { get { return _leafElement; } }

        [IsDumpEnabled(false)]
        internal override bool IsEmpty { get { return LeafElement.IsEmpty; } }

        [IsDumpEnabled(false)]
        internal override RefAlignParam RefAlignParam { get { return LeafElement.RefAlignParam; } }

        internal override CodeBase CreateChild(LeafElement leafElement)
        {
            if(!TryToCombine)
                return base.CreateChild(leafElement);

            var newLeafElements = LeafElement.TryToCombineN(leafElement);
            if(newLeafElements == null)
                return base.CreateChild(leafElement);

            var startingLeafElement = newLeafElements[0] as StartingLeafElement;
            Tracer.Assert(startingLeafElement != null);
            CodeBase result = new Leaf(startingLeafElement);
            for(var i = 1; i < newLeafElements.Length; i++)
                result = result.CreateChild(newLeafElements[i]);
            return result;
        }

        protected override TResult VisitImplementation<TResult>(Visitor<TResult> actual)
        {
            return actual.Leaf((LeafElement) LeafElement);
        }

        internal override BitsConst Evaluate()
        {
            return LeafElement.Evaluate();
        }
    }
}