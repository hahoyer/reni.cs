using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Code;
using Reni.Context;
using Reni.Feature;

namespace Reni.Type
{
    interface IReference : IContextReference
    {
        IConversion Converter { get; }
        bool IsWeak { get; }
    }
}