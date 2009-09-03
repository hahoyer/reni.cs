using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    [Token("type")]
    internal sealed class TtypeT : Defineable, IFeature
    {
        bool IFeature.IsEval { get { return false; } }
        TypeBase IFeature.ResultType { get { return null; } }
        Result IFeature.Apply(Category category, Result objectResult) { return objectResult.Type.AutomaticDereference().TypeOperator(category); }
    }

    [Token("dump_print")]
    internal sealed class DumpPrint :
        Defineable,
        IFeature,
        ISearchPath<ISearchPath<IFeature, Sequence>, Bit>
    {
        private readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();

        private sealed class BitSequenceFeature :
            ReniObject,
            ISearchPath<IFeature, Sequence>,
            IFeature
        {
            IFeature ISearchPath<IFeature, Sequence>.Convert(Sequence type) { return this; }
            bool IFeature.IsEval { get { return true; } }
            TypeBase IFeature.ResultType { get { return TypeBase.CreateVoid; } }
            Result IFeature.Apply(Category category, Result objectResult)
            {
                NotImplementedMethod(category,objectResult);
                return objectResult.DumpPrintBitSequence() & category;
            }
        }

        ISearchPath<IFeature, Sequence> ISearchPath<ISearchPath<IFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }

        bool IFeature.IsEval { get { return true; } }
        TypeBase IFeature.ResultType { get { return TypeBase.CreateVoid; } }

        Result IFeature.Apply(Category category, Result objectResult)
        {
            NotImplementedMethod(category, objectResult);
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