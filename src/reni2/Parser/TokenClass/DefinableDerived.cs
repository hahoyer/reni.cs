using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    [Token("type")]
    internal sealed class TtypeT : Defineable, ISuffixFeature
    {
        bool IUnaryFeature.IsEval { get { return false; } }
        TypeBase IUnaryFeature.ResultType { get { return null; } }
        Result IUnaryFeature.Apply(Category category, Result objectResult) { return objectResult.Type.AutomaticDereference().TypeOperator(category); }
    }

    [Token("dump_print")]
    internal sealed class DumpPrint :
        Defineable,
        ISuffixFeature,
        ISearchPath<ISearchPath<ISuffixFeature, Sequence>, Bit>
    {
        private readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();

        private sealed class BitSequenceFeature :
            ReniObject,
            ISearchPath<ISuffixFeature, Sequence>,
            ISuffixFeature
        {
            ISuffixFeature ISearchPath<ISuffixFeature, Sequence>.Convert(Sequence type) { return this; }
            bool IUnaryFeature.IsEval { get { return true; } }
            TypeBase IUnaryFeature.ResultType { get { return TypeBase.CreateVoid; } }
            Result IUnaryFeature.Apply(Category category, Result objectResult)
            {
                NotImplementedMethod(category,objectResult);
                return objectResult.DumpPrintBitSequence() & category;
            }
        }

        ISearchPath<ISuffixFeature, Sequence> ISearchPath<ISearchPath<ISuffixFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }

        bool IUnaryFeature.IsEval { get { return true; } }
        TypeBase IUnaryFeature.ResultType { get { return TypeBase.CreateVoid; } }

        Result IUnaryFeature.Apply(Category category, Result objectResult)
        {
            NotImplementedMethod(category, objectResult);
            return null;
        }
    }

    [Token("enable_cut")]
    internal sealed class EnableCut : Defineable, ISearchPath<ISuffixFeature, Sequence>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, Sequence>.Convert(Sequence type) { return type.EnableCutFeature; }
    }

    [Token("<<")]
    internal sealed class ConcatArrays : Defineable, ISearchPath<ISuffixFeature, Type.Array>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArraysFeature(type); }
    }

    [Token("<*")]
    internal sealed class ConcatArrayWithObject : Defineable, ISearchPath<ISuffixFeature, Type.Array>, ISearchPath<ISuffixFeature, Type.Void>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArrayWithObjectFeature(type); }
        ISuffixFeature ISearchPath<ISuffixFeature, Type.Void>.Convert(Type.Void type) { return new CreateArrayFeature(); }
    }

    [Token(":=")]
    internal sealed class ColonEqual : Defineable, ISearchPath<ISuffixFeature, AssignableRef>
    {
        ISuffixFeature ISearchPath<ISuffixFeature, AssignableRef>.Convert(AssignableRef type) { return type.AssignmentFeature; }
    }
}