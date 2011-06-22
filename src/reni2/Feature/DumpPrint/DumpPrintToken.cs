using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Sequence;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal sealed class DumpPrintToken :
        Defineable,
        IFeature,
        ISearchPath<ISearchPath<IFeature, Reference>, StructureType>,
        ISearchPath<IFeature, TypeType>,
        ISearchPath<IFeature, Bit>,
        ISearchPath<IFeature, Type.Void>,
        ISearchPath<IFeature, StructureType>,
        ISearchPath<ISearchPath<IFeature, BaseType>, Bit>,
        ISearchPath<IFeature, FunctionalFeatureType>
    {
        private static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        private static readonly BitFeature _bitFeature = new BitFeature();

        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return new Feature(type.DumpPrintResult); }
        IFeature ISearchPath<IFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return new Feature(type.Result); }
        IFeature ISearchPath<IFeature, StructureType>.Convert(StructureType type) { return new Feature(type.DumpPrintResult); }

        IFeature ISearchPath<IFeature, FunctionalFeatureType>.Convert(FunctionalFeatureType type) { return new Feature(type.DumpPrintResult); }

        ISearchPath<IFeature, BaseType> ISearchPath<ISearchPath<IFeature, BaseType>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, Reference> ISearchPath<ISearchPath<IFeature, Reference>, StructureType>.Convert(StructureType type) { return type.DumpPrintReferenceFeature; }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam)
        {
            NotImplementedMethod(category);
            return null;
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }
    }
}