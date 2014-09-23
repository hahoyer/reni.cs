using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Type;

namespace Reni
{
    interface IProxyType
    {
        IConverter Converter { get; }
    }
}