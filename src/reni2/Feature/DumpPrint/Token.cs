using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Struct;
using Reni.TokenClasses;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal sealed class Token :
        Defineable,
        IFeature,
        ISearchPath<ISearchPath<IFeature, Reference>, Struct.Type>,
        ISearchPath<IFeature, TypeType>,
        ISearchPath<IFeature, Bit>,
        ISearchPath<IFeature, Type.Void>,
        ISearchPath<IFeature, Struct.Type>,
        ISearchPath<IFeature, Field>,
        ISearchPath<ISearchPath<IFeature, Type.Sequence>, Bit>,
        ISearchPath<IFeature, FunctionalFeatureType>
    {
        private static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        private static readonly BitFeature _bitFeature = new BitFeature();

        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return new Feature(type.DumpPrintResult); }
        IFeature ISearchPath<IFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return new Feature(type.Result); }
        IFeature ISearchPath<IFeature, Struct.Type>.Convert(Struct.Type type) { return new Feature(type.CreateDumpPrintResult); }
        IFeature ISearchPath<IFeature, Field>.Convert(Field type) { return new Feature(type.DumpPrintResult); }
        IFeature ISearchPath<IFeature, FunctionalFeatureType>.Convert(FunctionalFeatureType type) { return new Feature(type.CreateDumpPrintResult); }

        ISearchPath<IFeature, Type.Sequence> ISearchPath<ISearchPath<IFeature, Type.Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, Reference> ISearchPath<ISearchPath<IFeature, Reference>, Struct.Type>.Convert(Struct.Type type) { return type.DumpPrintReferenceFeature; }

        Result IFeature.Apply(Category category)
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