using System;
using System.Linq;
using System.Collections.Generic;

namespace Reni.Code
{
    internal class HasArgVisitor : Visitor<bool>
    {
        internal override bool Pair(Pair visitedObject, bool left, bool right) { return left || right; }
        internal override bool Arg(Arg visitedObject) { return true; }
        internal override bool Child(bool parent, LeafElement leafElement) { return parent; }
        internal override bool Leaf(LeafElement leafElement) { return false; }
        internal override bool InternalRef(InternalRef visitedObject) { return false; }
    }
}