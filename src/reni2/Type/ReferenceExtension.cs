using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Code;
using Reni.Context;

namespace Reni.Type
{
    static class ReferenceExtension
    {
        internal static TypeBase Type(this IReference target) => (TypeBase) target;

        internal static Size Size(this IContextReference target)
            => (target as TypeBase)?.Size
                ?? Root.DefaultRefAlignParam.RefSize;
    }
}