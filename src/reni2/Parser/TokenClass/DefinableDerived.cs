using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    [Token("type")]
    internal sealed class TtypeT : Defineable, ISearchPath<IFeature,TypeBase>
    {
        IFeature ISearchPath<IFeature, TypeBase>.Convert(TypeBase type)
        {
            NotImplementedMethod(type);
            return null;
        }
    }

    [Token("dump_print")]
    internal sealed class DumpPrint :
        Defineable,
        IFeature,
        ISearchPath<IFeature, TypeType>,
        ISearchPath<ISearchPath<IFeature, Ref>, Struct.Type>,
        ISearchPath<IFeature, Bit>,
        ISearchPath<IFeature, Type.Void>,
        ISearchPath<ISearchPath<IFeature, Sequence>, Bit>
    {
        private sealed class BitFeature : BitFeatureBase, IFeature
        {
            Result IFeature.Apply(Category category)
            {
                return Apply(category, 1).UseWithArg(TypeBase.CreateBit.CreateArgResult(category).Align(BitsConst.SegmentAlignBits));
            }
        }

        private static readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();
        private static readonly BitFeature _bitFeature = new BitFeature();
        private static readonly VoidFeature _voidFeature = new VoidFeature();

        private class VoidFeature: ReniObject, IFeature
        {
            Result IFeature.Apply(Category category) { return TypeBase.CreateVoidResult(category); }
        }

        private sealed class BitSequenceFeature :
            ReniObject,
            ISearchPath<IFeature, Sequence>
        {
            IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return type.BitDumpPrintFeature; }
        }

        ISearchPath<IFeature, Sequence> ISearchPath<ISearchPath<IFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }
        ISearchPath<IFeature, Ref> ISearchPath<ISearchPath<IFeature, Ref>, Struct.Type>.Convert(Struct.Type type) { return type.DumpPrintFromRefFeature; }
        IFeature ISearchPath<IFeature, TypeType>.Convert(TypeType type) { return type.DumpPrintFeature; }
        IFeature ISearchPath<IFeature, Bit>.Convert(Bit type) { return _bitFeature; }
        IFeature ISearchPath<IFeature, Type.Void>.Convert(Type.Void type) { return _voidFeature; }

        Result IFeature.Apply(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        internal abstract class BitFeatureBase : ReniObject
        {
            protected static Result Apply(Category category, int objSize) { return TypeBase.CreateVoid.CreateResult(category, () => CodeBase.CreateBitSequenceDumpPrint(objSize)); }
        }

        internal class BitSequenceFeatureClass : BitFeatureBase, IFeature
        {
            private readonly Sequence _parent;

            public BitSequenceFeatureClass(Sequence parent)
            {
                _parent = parent;
            }

            Result IFeature.Apply(Category category)
            {
                return Apply(category, _parent.SequenceCount).UseWithArg(_parent.CreateAlign(BitsConst.SegmentAlignBits).CreateArgResult(category));
            }
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