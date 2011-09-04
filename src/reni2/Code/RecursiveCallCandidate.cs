using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.TreeStructure;
using Reni.Basics;

namespace Reni.Code
{
    /// <summary>
    ///     Code element for a call that has been resolved as simple recursive call candidate. 
    ///     This implies, that the call is contained in the function called. 
    ///     It must not have any argument and should return nothing. 
    ///     It will be assembled as a jump to begin of function.
    /// </summary>
    [Serializable]
    internal sealed class RecursiveCallCandidate : FiberItem
    {
        private readonly Size _refsSize;

        internal override Size InputSize { get { return _refsSize; } }

        internal override Size OutputSize { get { return Size.Zero; } }

        internal override void Visit(IVisitor visitor) { throw new NotImplementedException(); }

        internal override CodeBase TryToCombineBack(TopFrameData precedingElement)
        {
            if((DeltaSize + precedingElement.Size).IsZero
               && (precedingElement.Offset + _refsSize).IsZero)
                return new RecursiveCall();
            return base.TryToCombineBack(precedingElement);
        }

        internal RecursiveCallCandidate(Size refsSize) { _refsSize = refsSize; }
    }
}