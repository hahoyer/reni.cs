using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using Reni.Basics;

namespace Reni.Code.ReplaceVisitor
{
    sealed class ReplaceRelRefArg : ReplaceArg
    {
        ReplaceRelRefArg(ResultCache actualArg, Size offset)
            : base(actualArg)
        {
            Offset = offset;
            StopByObjectIds(-9);
        }

        internal ReplaceRelRefArg(ResultCache actualArg)
            : this(actualArg, Size.Create(0)) {}

        [EnableDump]
        Size Offset { get; }

        [DisableDump]
        protected override CodeBase ActualCode
            => Offset.IsZero ? ActualArg.Code : ActualArg.Code.ReferencePlus(Offset);

        protected override Visitor<CodeBase, FiberItem> After(Size size)
            => new ReplaceRelRefArg(ActualArg, Offset + size);
    }
}