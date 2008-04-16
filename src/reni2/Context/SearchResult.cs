using HWClassLibrary.Debug;
using System;
using Reni.Type;

namespace Reni.Context
{
    internal abstract class SearchResult : ReniObject
    {
        virtual internal Result VisitApply(Base context, Category category, Syntax.Base args, Ref objectType)
        {
            NotImplementedMethod(context, category, args, objectType);
            return null;
        }
    }

    internal class DefaultSearchResultFromRef 
    {
        private readonly SearchResult _searchResult;

        public DefaultSearchResultFromRef(SearchResult searchResult, Ref definingType)
        {
            _searchResult = searchResult;
        }

        //internal override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        //{
        //    Result result = _searchResult.VisitApply(callContext, category, args);
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

