using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Basics;

namespace Reni.Type
{
    interface IRepeaterType
    {
        TypeBase ElementType { get; }
        Size IndexSize { get; }
        bool IsMutable { get; }
    }
}