using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using hw.Forms;
using Reni.Basics;
using Reni.Context;

namespace Reni.Code
{
    /// <summary>
    ///     Reference to something
    /// </summary>
    abstract class Ref : FiberHead
    {
        [Node]
        [DisableDump]
        internal readonly Size Offset;

        protected Ref(Size offset) { Offset = offset; }

        protected override Size GetSize() => Root.DefaultRefAlignParam.RefSize;

        protected override string GetNodeDump() => base.GetNodeDump() + " Offset=" + Offset;

        [Node]
        [DisableDump]
        internal override bool IsRelativeReference => true;
    }
}