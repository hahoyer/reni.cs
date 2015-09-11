using System;
using System.Collections.Generic;
using System.Linq;

namespace Reni.Code
{
    sealed class HasArgVisitor : Visitor<bool, bool>
    {
        internal override bool Arg(Arg visitedObject) => true;
        internal override bool BitArray(BitArray visitedObject) => false;
        internal override bool ContextRef(ReferenceCode visitedObject) => false;
        internal override bool Fiber(Fiber visitedObject) => visitedObject.HasArg;

        internal override bool List(List visitedObject)
            => visitedObject.Data.Any(x => x.Visit(this));

        internal override bool Default(CodeBase codeBase) => false;

        internal override bool LocalReference(LocalReference visitedObject)
            => visitedObject.AlignedValueCode.Visit(this);
    }
}