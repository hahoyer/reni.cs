using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    interface IReference : IContextReference
    {
        ISimpleFeature Converter { get; }
        bool IsWeak { get; }
    }

    static class ReferenceExtension
    {
        internal static TypeBase Type(this IReference referenceType) => (TypeBase) referenceType;
    }
}