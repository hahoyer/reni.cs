using System;
using System.Linq;
using System.Collections.Generic;
using hw.Debug;

namespace Reni.Code
{
    internal sealed class HasArgVisitor : Visitor<bool>
    {
        internal override bool Arg(Arg visitedObject) => true;
        internal override bool BitArray(BitArray visitedObject) => false;
        internal override bool ContextRef(ReferenceCode visitedObject) => false;
        internal override bool Fiber(Fiber visitedObject) => visitedObject.HasArg;
        internal override bool List(List visitedObject) { return visitedObject.Data.Any(x => x.Visit(this)); }
        internal override bool Default(CodeBase codeBase) => false;
        internal override bool LocalReference(LocalReference visitedObject) => visitedObject.AlignedValueCode.Visit(this);
    }
}