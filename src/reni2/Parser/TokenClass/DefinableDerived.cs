using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
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
        ISearchPath<ISearchPath<IInfixFeature, Sequence>, Bit>
    {
        private readonly BitSequenceFeature _bitSequenceFeature = new BitSequenceFeature();

        private sealed class BitSequenceFeature : ReniObject, ISearchPath<IInfixFeature, Sequence>
        {
            IInfixFeature ISearchPath<IInfixFeature, Sequence>.Convert(Sequence type) { return type.BitOperationFeature(this); }
        }

        Result IInfixFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);
            if(category.HasCode || category.HasRefs)
                return callContext.ApplyResult(category, @object, @ref => @ref.DumpPrint(category));
            return Type.Void.CreateResult(category);
        }

        ISearchPath<IInfixFeature, Sequence> ISearchPath<ISearchPath<IInfixFeature, Sequence>, Bit>.Convert(Bit type) { return _bitSequenceFeature; }

        bool IUnaryFeature.IsEval { get { return true; } }
        TypeBase IUnaryFeature.ResultType { get { return TypeBase.CreateVoid; } }

        Result IUnaryFeature.Apply(Category category, Result objectResult) { throw new NotImplementedException(); }
    }

    internal interface ISequenceOfBitDumpPrint : ISequenceOfBitOperation
    {
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