using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;

namespace Reni.Code
{
    sealed class LocalVariableDefinition : FiberItem
    {
        [Node]
        readonly Holder _holder;
        readonly Size _valueSize;

        public LocalVariableDefinition(Holder holder, Size valueSize)
        {
            _holder = holder;
            _valueSize = valueSize;
            StopByObjectId(-4);
        }

        [DisableDump]
        internal override Size InputSize => _valueSize;

        [DisableDump]
        internal override Size OutputSize => Size.Zero;

        protected override string GetNodeDump() => base.GetNodeDump() + " Holder=" + _holder + " ValueSize=" + _valueSize;
        internal override void Visit(IVisitor visitor) => visitor.LocalVariableDefinition(_holder.Name, _valueSize);
    }
}