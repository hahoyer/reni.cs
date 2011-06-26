using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Sequence;
using Reni.Type;


namespace Reni.TokenClasses
{
    internal sealed class EnableCut : Defineable, ISearchPath<IFeature, BaseType>
    {
        IFeature ISearchPath<IFeature, BaseType>.Convert(BaseType type) { return new Feature.Feature(type.EnableCutFeature); }
    }

    internal sealed class ConcatArrays : Defineable, ISearchPath<IFeature, Type.Array>
    {
        IFeature ISearchPath<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArraysFeature(type); }
    }

    internal sealed class ConcatArrayWithObject : Defineable, ISearchPath<IFeature, Type.Array>, ISearchPath<IFeature, Type.Void>
    {
        IFeature ISearchPath<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArrayWithObjectFeature(type); }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return new CreateArrayFeature(); }
    }

    internal sealed class Assignment : Defineable, ISearchPath<IFeature, AutomaticReferenceType>
    {
        IFeature ISearchPath<IFeature, AutomaticReferenceType>.Convert(AutomaticReferenceType type) { return new Feature.Feature(type.ApplyAssignment); }
    }
}