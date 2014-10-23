using System.Linq;
using System.Collections.Generic;
using System;
using Reni.Code;
using Reni.Feature;

namespace Reni.Type
{
    interface IReferenceType : IContextReference
    {
        ISimpleFeature Converter { get; }
        bool IsWeak { get; }
    }

    static class ReferenceExtension
    {
        internal static TypeBase Type(this IReferenceType referenceType) { return (TypeBase) referenceType; }
    }
}