using System;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;
using Void=Reni.Type.Void;

namespace Reni.Parser.TokenClass.Name
{
    [Token("dump_print"), Serializable]
    internal sealed class DumpPrint : Defineable, IFeature
    {
        internal override SearchResult<IFeature> Search()
        {
            return SearchResult<IFeature>.Success(this, this);
        }

        public Result Result(ContextBase callContext, Category category, ICompileSyntax args, AutomaticRef callObject)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, args, callObject);

            return callObject.DumpPrint(category);
        }

        public Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                  ICompileSyntax args)
        {
            if(args != null)
                NotImplementedMethod(callContext, category, @object, args);
            if(category.HasCode || category.HasRefs)
                return callContext.ApplyResult(category, @object, ot => ot.DumpPrint(category));
            return Void.CreateResult(category);
        }
    }
}