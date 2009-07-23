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
    internal sealed class TtypeT : Defineable, IFeature
    {
        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                  ICompileSyntax args)
        {
            var objectType = callContext.Type(@object).AutomaticDereference();
            if(args == null)
                return objectType.TypeOperator(category);
            return callContext.ApplyResult(category, args, argsType => argsType.Conversion(category, objectType));
        }
    }

    [Token("dump_print")]
    internal sealed class DumpPrint : Defineable,
                                      IFeature,
                                      IConverter<IConverter<IFeature, Sequence>, Bit>
    {
        private sealed class BitFeature : ReniObject, IConverter<IFeature, Sequence>, ISequenceOfBitDumpPrint
        {
            IFeature IConverter<IFeature, Sequence>.Convert(Sequence type)
            {
                return type.BitOperationFeature(
                    this);
            }

            TypeBase ISequenceOfBitOperation.ResultType(int objBitCount) { return TypeBase.CreateVoid; }
            string ISequenceOfBitOperation.DataFunctionName { get { return "DumpPrint"; } }
            public Result SequenceOperationResult(Category category, TypeBase typeBase, Size objSize) { return typeBase.DumpPrint() }

            string ISequenceOfBitOperation.CSharpNameOfDefaultOperation { get { return ""; } }
        }

        private readonly BitFeature _bitFeature = new BitFeature();

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                  ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);
            if(category.HasCode || category.HasRefs)
                return callContext.ApplyResult(category, @object, ot => ot.DumpPrint(category));
            return Type.Void.CreateResult(category);
        }

        IConverter<IFeature, Sequence> IConverter<IConverter<IFeature, Sequence>, Bit>.Convert(Bit type) { return _bitFeature; }
    }

    internal interface ISequenceOfBitDumpPrint : ISequenceOfBitOperation
    {
    }

    [Token("enable_cut")]
    internal sealed class EnableCut : Defineable, IConverter<IFeature, Sequence>
    {
        IFeature IConverter<IFeature, Sequence>.Convert(Sequence type) { return type.EnableCutFeature; }
    }

    [Token("<<")]
    internal sealed class ConcatArrays : Defineable, IConverter<IFeature, Type.Array>
    {
        IFeature IConverter<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArraysFeature(type); }
    }

    [Token("<*")]
    internal sealed class ConcatArrayWithObject : Defineable, IConverter<IFeature, Type.Array>, IConverter<IFeature, Type.Void>
    {
        IFeature IConverter<IFeature, Type.Array>.Convert(Type.Array type) { return new ConcatArrayWithObjectFeature(type); }
        IFeature IConverter<IFeature, Type.Void>.Convert(Type.Void type) { return new CreateArrayFeature(); }
    }

    [Token(":=")]
    internal sealed class ColonEqual : Defineable, IConverter<IFeature, AssignableRef>
    {
        IFeature IConverter<IFeature, AssignableRef>.Convert(AssignableRef type) { return type.AssignmentFeature; }
    }
}