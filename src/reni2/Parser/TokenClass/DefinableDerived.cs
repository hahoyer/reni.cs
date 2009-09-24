using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    [Token("type")]
    internal sealed class TtypeT : Defineable, IFeature
    {
       Result IFeature.Apply(Category category, TypeBase objectType) { return objectType.AutomaticDereference().TypeOperator(category); }
    }

    [Token("dump_print")]
    internal sealed class DumpPrint :
        Defineable,
        IFeature,
        ISearchPath<IFeature, TypeType>,
        ISearchPath<ISearchPath<IFeature, Ref>, Struct.Type>,
        ISearchPath<ISearchPath<IFeature, Sequence>, Bit>
    {
        private readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();

        private sealed class BitSequenceFeature :
            ReniObject,
            ISearchPath<IFeature, Sequence>,
            IFeature
        {
            IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return this; }

            private static Result Apply(Category category, int objSize) { return TypeBase.CreateVoid.CreateResult(category, () => CodeBase.CreateBitSequenceDumpPrint(objSize)); }

            Result IFeature.Apply(Category category, TypeBase objectType) { return Apply(category, objectType.SequenceCount).UseWithArg(objectType.ConvertToBitSequence(category)); }
        }

        ISearchPath<IFeature, Sequence> ISearchPath<ISearchPath<IFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, Ref> ISearchPath<ISearchPath<IFeature, Ref>, Struct.Type>.Convert(Struct.Type type) { return type.DumpPrintFromRefFeature; }
        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return type.DumpPrintFeature; }

        Result IFeature.Apply(Category category, TypeBase objectType)
        {
            NotImplementedMethod(category, objectType);
            return null;
        }
    }

    [Token("enable_cut")]
    internal sealed class EnableCut : Defineable, ISearchPath<IFeature, Sequence>
    {
        IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return type.EnableCutFeature; }
    }

    [Token("<<")]
    internal sealed class ConcatArrays : Defineable, ISearchPath<IFeature, Type.Array>
    {
        IFeature ISearchPath<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArraysFeature(type); }
    }

    [Token("<*")]
    internal sealed class ConcatArrayWithObject : Defineable, ISearchPath<IFeature, Type.Array>, ISearchPath<IFeature, Type.Void>
    {
        IFeature ISearchPath<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArrayWithObjectFeature(type); }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return new CreateArrayFeature(); }
    }

    [Token(":=")]
    internal sealed class ColonEqual : Defineable, ISearchPath<IFeature, AssignableRef>
    {
        IFeature ISearchPath<IFeature, AssignableRef>.Convert(AssignableRef type) { return type.AssignmentFeature; }
    }
}