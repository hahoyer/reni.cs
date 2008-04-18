using HWClassLibrary.Debug;
using System;
using Reni.Type;

namespace Reni.Feature
{
    internal abstract class FeatureBase : ReniObject
    {
        virtual internal Result VisitApply(Context.Base context, Category category, Syntax.Base args, Ref objectType)
        {
            NotImplementedMethod(context, category, args, objectType);
            return null;
        }
    }

    internal class DefaultSearchResultFromRef 
    {
        private readonly FeatureBase _featureBase;

        public DefaultSearchResultFromRef(FeatureBase featureBase, Ref definingType)
        {
            _featureBase = featureBase;
        }

        //internal override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        //{
        //    Result result = _featureBase.VisitApply(callContext, category, args);
        //    result = result.UseWithArg(DefiningType.CreateDereferencedArgResult(category));
        //    return result;
        //}
    }

    abstract internal class PrefixSearchResult : ReniObject
    {
        internal virtual Result VisitApply(Category category, Result argResult)
        {
            NotImplementedMethod(category, argResult);
            throw new NotImplementedException();
        }
    }
}