using HWClassLibrary.Debug;
using System;
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
    internal sealed class DumpPrint : Defineable, IFeature
    {
        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                  ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);
            if(category.HasCode || category.HasRefs)
                return callContext.ApplyResult(category, @object, ot => ot.DumpPrint(category));
            return Type.Void.CreateResult(category);
        }
    }

    [Token("enable_cut")]
    internal sealed class EnableCut : Defineable, IConverter<IFeature, Sequence>
    {
        IFeature IConverter<IFeature, Sequence>.Convert(Sequence type) { return type.EnableCutFeature; }
    }

    [Token("<<")]
    internal sealed class ConcatArrays : Defineable, IConverter<IFeature, IArray>
    {
        IFeature IConverter<IFeature, IArray>.Convert(IArray type) { return new ConcatArraysFeature(type); }
    }

    [Token("<*")]
    internal sealed class ConcatArrayWithObject : Defineable, IConverter<IFeature, IArray>
    {
        IFeature IConverter<IFeature, IArray>.Convert(IArray type) { return new ConcatArrayWithObjectFeature(type); }
    }

    [Token(":=")]
    internal sealed class ColonEqual : Defineable, IConverter<IFeature, AssignableRef>
    {
        IFeature IConverter<IFeature, AssignableRef>.Convert(AssignableRef type) { return type.AssignmentFeature; }
    }

}