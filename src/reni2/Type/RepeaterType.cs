using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;

namespace Reni.Type
{
    interface IRepeaterType
    {
        TypeBase ElementType { get; }
        TypeBase IndexType { get; }
        bool IsMutable { get; }
    }
}