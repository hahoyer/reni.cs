using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.Basics;

namespace Reni.Code
{
    sealed class Drop : FiberItem
    {
        readonly Size _beforeSize;
        readonly Size _afterSize;

        public Drop(Size beforeSize, Size afterSize)
        {
            _beforeSize = beforeSize;
            _afterSize = afterSize;
        }

        internal override void Visit(IVisitor visitor) => visitor.Drop(_beforeSize, _afterSize);

        [DisableDump]
        internal override Size InputSize => _beforeSize;

        [DisableDump]
        internal override Size OutputSize => _afterSize;

        protected override string GetNodeDump()
            => base.GetNodeDump() + " BeforeSize=" + _beforeSize + " AfterSize=" + _afterSize;

        protected override TFiber VisitImplementation<TCode, TFiber>(Visitor<TCode, TFiber> actual)
            => actual.Drop(this);
    }
}