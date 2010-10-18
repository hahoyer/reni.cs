using System;
using System.Linq;
using System.Collections.Generic;

namespace Reni.Code
{
    internal sealed class HasArgVisitor : Visitor<bool>
    {
        protected override Visitor<bool> AfterThen(Size theSize) { return this; }
        protected override Visitor<bool> AfterCond() { return this; }
        protected override Visitor<bool> AfterElse() { return this; }

        internal override bool FiberHead(FiberHead visitedObject) { return false; }
        internal override bool Arg(Arg visitedObject) { return true; }
        internal override bool ContextRef(ReferenceCode visitedObject) { return false; }
        protected override bool Fiber(Fiber visitedObject, bool head) { return head; }
        internal override bool LocalReference(LocalReference visitedObject) { return false; }
    }
}