using HWClassLibrary.Debug;
using System;
using Reni.Type;

namespace Reni.Context
{
    /// <summary>
    /// Result of token search.
    /// </summary>
    internal abstract class SearchResult : ReniObject
    {
        [DumpData(true)]
        private readonly Type.Base _definingType;

        public SearchResult(Type.Base definingType)
        {
            _definingType = definingType;
        }

        /// <summary>
        /// Gets the type that is defines the target
        /// </summary>
        /// <value>The type of the defining.</value>
        /// created 09.04.2007 13:15 on SAPHIRE by HH
        public Type.Base DefiningType { get { return _definingType; } }

        /// <summary>
        /// Creates the result for member function searched. Object is provided by use of "Arg" code element
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        protected internal abstract Result VisitApply(Base callContext, Category category, Syntax.Base args);

        public SearchResultFromRef ToSearchResultFromRef(Ref definingType)
        {
            return new DefaultSearchResultFromRef(this,definingType);
        }
    }

    internal class DefaultSearchResultFromRef : SearchResultFromRef
    {
        private readonly SearchResult _searchResult;

        public DefaultSearchResultFromRef(SearchResult searchResult, Ref definingType):base(definingType)
        {
            _searchResult = searchResult;
        }

        internal override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            Result result = _searchResult.VisitApply(callContext, category, args);
            result = result.UseWithArg(DefiningType.CreateDereferencedArgResult(category));
            return result;
        }
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

