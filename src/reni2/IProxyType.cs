using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Feature;

namespace Reni
{
    interface IProxyType
    {
        IValueFeature Converter { get; }
    }
}