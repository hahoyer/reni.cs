﻿using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Parser.TokenClass;
using Reni.Struct;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    internal sealed class Token :
        Defineable,
        IFeature,
        ISearchPath<ISearchPath<IFeature, Type.Reference>,Struct.Type>,
        ISearchPath<IFeature, TypeType>,
        ISearchPath<IFeature, Bit>,
        ISearchPath<IFeature, Type.Void>,
        ISearchPath<IFeature, Struct.Type>,
        ISearchPath<IFeature, Struct.Reference>,
        ISearchPath<ISearchPath<IFeature, Sequence>, Bit>,
        ISearchPath<IFeature, FunctionDefinitionType>
    {
        private static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        private static readonly BitFeature _bitFeature = new BitFeature();

        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return new Feature(type.ApplyDumpPrintFeature); }
        IFeature ISearchPath<IFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return new Feature(type.CreateResult); }
        IFeature ISearchPath<IFeature, Struct.Type>.Convert(Struct.Type type) { return type.DumpPrintFeature; }
        IFeature ISearchPath<IFeature, Struct.Reference>.Convert(Struct.Reference type) { return type.DumpPrintFeature; }
        IFeature ISearchPath<IFeature, FunctionDefinitionType>.Convert(FunctionDefinitionType type) { return new Feature(type.DumpPrintFeatureApply); }

        ISearchPath<IFeature, Sequence> ISearchPath<ISearchPath<IFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, Type.Reference> ISearchPath<ISearchPath<IFeature, Type.Reference>, Struct.Type>.Convert(Struct.Type type) { return type.DumpPrintReferenceFeature; }

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