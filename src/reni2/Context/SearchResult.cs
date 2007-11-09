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
        private readonly Ref _definingType;

        public DefaultSearchResultFromRef(SearchResult searchResult, Ref definingType)
        {
            _searchResult = searchResult;
            _definingType = definingType;
        }

        internal override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            Result result = _searchResult.VisitApply(callContext, category, args);
            result = result.UseWithArg(_definingType.CreateDereferencedArgResult(category));
            return result;
        }
    }

    internal abstract class StructSearchResult : ReniObject
    {
        internal virtual Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            NotImplementedMethod(callContext, category, args);
            return null;
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

