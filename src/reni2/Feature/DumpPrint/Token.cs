using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Parser.TokenClass;
using Reni.Type;

namespace Reni.Feature.DumpPrint
{
    [Token("dump_print")]
    internal sealed class Token :
        Defineable,
        IFeature,
        ISearchPath<IFeature, TypeType>,
        ISearchPath<IFeature, Bit>,
        ISearchPath<IFeature, Type.Void>,
        ISearchPath<ISearchPath<IFeature, Ref>, Struct.Type>,
        ISearchPath<ISearchPath<IFeature, Sequence>, Bit>
    {
        private static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        private static readonly BitFeature _bitFeature = new BitFeature();
        private static readonly VoidFeature _voidFeature = new VoidFeature();

        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return type.DumpPrintFeature; }
        IFeature ISearchPath<IFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return _voidFeature; }

        ISearchPath<IFeature, Sequence> ISearchPath<ISearchPath<IFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, Ref> ISearchPath<ISearchPath<IFeature, Ref>, Struct.Type>.Convert(Struct.Type type) { return type.DumpPrintFromRefFeature; }

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