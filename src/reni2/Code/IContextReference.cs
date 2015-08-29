using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;

namespace Reni.Code
{
    interface IContextReference
    {
        Size Size { get; }
        int Order { get; }
    }

    interface IContextReferenceProvider
    {
        IContextReference ContextReference { get; }
    }
}