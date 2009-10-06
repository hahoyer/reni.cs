using System;
using System.Linq;
using System.Collections.Generic;

namespace Reni.Code
{
    internal class HasArgVisitor : Visitor<bool>
    {
        internal override bool Pair(Pair visitedObject, bool left, bool right) { return left || right; }
        internal override Visitor<bool> AfterThen(int objectId, Size theSize) { return this; }
        internal override Visitor<bool> AfterCond(int objectId) { return this; }
        internal override Visitor<bool> AfterElse(int objectId) { return this; }
        internal override bool Arg(Arg visitedObject) { return true; }
        internal override bool ContextRef(RefCode visitedObject) { return false; }
        internal override bool Child(bool parent, LeafElement leafElement) { return parent; }
        internal override bool Leaf(LeafElement leafElement) { return false; }
        internal override bool InternalRef(InternalRef visitedObject) { return false; }
        internal override bool ThenElse(ThenElse visitedObject, bool condResult, bool thenResult, bool elseResult){return condResult || thenResult || elseResult;}
    }
}