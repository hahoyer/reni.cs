using System;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
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
// ReSharper disable RedundantDefaultFieldInitializer
        public static bool TryToCombine = false;
// ReSharper restore RedundantDefaultFieldInitializer

        internal Leaf(LeafElement leafElement)
        {
            _leafElement = leafElement;
            StopByObjectId(526);
        }

        internal protected override Size GetSize (){ return LeafElement.Size; } 
        [Node]
        internal LeafElement LeafElement { get { return _leafElement; } }
        internal override bool IsEmpty { get { return LeafElement.IsEmpty; } }
        internal override RefAlignParam RefAlignParam { get { return LeafElement.RefAlignParam; } }

        public override CodeBase CreateChild(LeafElement leafElement)
        {
            if (!TryToCombine)
                return base.CreateChild(leafElement);

            var newLeafElements = LeafElement.TryToCombineN(leafElement);
            if (newLeafElements == null)
                return base.CreateChild(leafElement);

            CodeBase result = new Leaf(newLeafElements[0]);
            for (var i = 1; i < newLeafElements.Length; i++ )
                result = result.CreateChild(newLeafElements[i]);
            return result;
        }

        public override Result VirtVisit<Result>(Visitor<Result> actual)
        {
            return actual.Leaf(LeafElement);
        }

        internal override BitsConst Evaluate()
        {
            return LeafElement.Evaluate();
        }
    }
}