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
        internal override bool Arg(Arg visitedObject) { return true; }
        internal override bool ContextRef(ReferenceCode visitedObject) { return false; }
        protected override bool Fiber(Fiber visitedObject, bool head) { return head; }
        internal override bool List(List visitedObject) { return visitedObject.Data.Any(x => x.Visit(this)); }
        internal override bool Default() { return false; }
        internal override bool LocalReference(LocalReference visitedObject) { return false; }
    }
}