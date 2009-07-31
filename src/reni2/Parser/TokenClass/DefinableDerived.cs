using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Feature;
using Reni.Type;

namespace Reni.Parser.TokenClass
{
    [Token("type")]
    internal sealed class TtypeT : Defineable, IInfixFeature, ISuffixFeature
    {
        bool IInfixFeature.IsEvalLeft { get { return false; } }
        TypeBase IInfixFeature.ResultType { get { return null; } }
        Result IInfixFeature.Apply(Category category, Result leftResult, Result rightResult) { return rightResult.ConvertTo(leftResult.Type.AutomaticDereference()); }

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
    internal sealed class EnableCut : Defineable, ISearchPath<IInfixFeature, Sequence>
    {
        IInfixFeature ISearchPath<IInfixFeature, Sequence>.Convert(Sequence type) { return type.EnableCutFeature; }
    }

    [Token("<<")]
    internal sealed class ConcatArrays : Defineable, ISearchPath<IInfixFeature, Type.Array>
    {
        IInfixFeature ISearchPath<IInfixFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArraysFeature(type); }
    }

    [Token("<*")]
    internal sealed class ConcatArrayWithObject : Defineable, ISearchPath<IInfixFeature, Type.Array>, ISearchPath<IInfixFeature, Type.Void>
    {
        IInfixFeature ISearchPath<IInfixFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArrayWithObjectFeature(type); }
        IInfixFeature ISearchPath<IInfixFeature, Type.Void>.Convert(Type.Void type) { return new CreateArrayFeature(); }
    }

    [Token(":=")]
    internal sealed class ColonEqual : Defineable, ISearchPath<IInfixFeature, AssignableRef>
    {
        IInfixFeature ISearchPath<IInfixFeature, AssignableRef>.Convert(AssignableRef type) { return type.AssignmentFeature; }
    }
}