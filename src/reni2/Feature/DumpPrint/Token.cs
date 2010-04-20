using System;
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
        ISearchPath<IFeature, FullContextType>,
        ISearchPath<ISearchPath<IFeature, Sequence>, Bit>
    {
        private static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        private static readonly BitFeature _bitFeature = new BitFeature();
        private static readonly VoidFeature _voidFeature = new VoidFeature();

        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return type.DumpPrintFeature; }
        IFeature ISearchPath<IFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return _voidFeature; }
        IFeature ISearchPath<IFeature, FullContextType>.Convert(FullContextType type) { return type.DumpPrintFeature; }

        ISearchPath<IFeature, Sequence> ISearchPath<ISearchPath<IFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, Type.Reference> ISearchPath<ISearchPath<IFeature, Type.Reference>, Struct.Type>.Convert(Struct.Type type) { return type.DumpPrintFeature; }

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