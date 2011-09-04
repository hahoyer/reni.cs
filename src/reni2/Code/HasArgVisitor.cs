using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal sealed class HasArgVisitor : Visitor<bool>
    {
        internal override bool Arg(Arg visitedObject) { return true; }
        internal override bool BitArray(BitArray visitedObject) { return false; }
        internal override bool ContextRef(ReferenceCode visitedObject) { return false; }
        internal override bool Fiber(Fiber visitedObject) { return visitedObject.HasArg; }
        internal override bool List(List visitedObject) { return visitedObject.Data.Any(x => x.Visit(this)); }
        internal override bool Default(CodeBase codeBase) { return false; }
        internal override bool LocalReference(LocalReference visitedObject) { return visitedObject.Code.Visit(this); }
    }
}