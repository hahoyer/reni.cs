using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Type;


namespace Reni.TokenClasses
{
    internal sealed class EnableCut : Defineable, ISearchPath<IFeature, Type.Sequence>
    {
        IFeature ISearchPath<IFeature, Type.Sequence>.Convert(Type.Sequence type) { return new Feature.Feature(type.EnableCutFeature); }
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

    internal sealed class Assignment : Defineable, ISearchPath<IFeature, Reference>
    {
        IFeature ISearchPath<IFeature, Reference>.Convert(Reference type) { return new Feature.Feature(type.ApplyAssignment); }
    }
}