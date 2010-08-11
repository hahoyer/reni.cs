using System;
using System.Linq;
using System.Collections.Generic;

namespace Reni.Code
{
    internal class HasArgVisitor : Visitor<bool>
    {
        internal override bool Pair(Pair visitedObject, bool left, bool right) { return left || right; }
        protected override Visitor<bool> AfterThen(Size theSize) { return this; }
        protected override Visitor<bool> AfterCond() { return this; }
        protected override Visitor<bool> AfterElse() { return this; }
        internal override bool Arg(Arg visitedObject) { return true; }
        internal override bool ContextRef(ReferenceCode visitedObject) { return false; }
        internal override bool Child(bool parent, LeafElement leafElement) { return parent; }
        internal override bool Leaf(LeafElement leafElement) { return false; }
        internal override bool LocalReference(LocalReference visitedObject) { return false; }
        internal override bool ThenElse(ThenElse visitedObject, bool condResult, bool thenResult, bool elseResult){return condResult || thenResult || elseResult;}
    }
}