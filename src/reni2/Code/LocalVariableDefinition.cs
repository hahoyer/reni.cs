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
        readonly string _holderName;
        readonly Size _valueSize;

        public LocalVariableDefinition(string holderName, Size valueSize)
        {
            _holderName = holderName;
            _valueSize = valueSize;
            StopByObjectId(-4);
        }

        [DisableDump]
        internal override Size InputSize => _valueSize;

        [DisableDump]
        internal override Size OutputSize => Size.Zero;

        protected override string GetNodeDump() => base.GetNodeDump() + " Holder=" + _holderName + " ValueSize=" + _valueSize;
        internal override void Visit(IVisitor visitor) => visitor.LocalVariableDefinition(_holderName, _valueSize);
    }
}