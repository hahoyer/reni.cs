using System;
using HWClassLibrary.Debug;

namespace Reni.Context
{
    /// <summary>
    /// Result of token search.
    /// </summary>
    public abstract class SearchResult : ReniObject
    {
        private Type.Base _definingType;

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
        public abstract Result VisitApply(Base callContext, Category category, Syntax.Base args);

        /// <summary>
        /// Creates the result for member function searched. Object is provided as reference by use of "Arg" code element
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public virtual Result VisitApplyFromRef(RefAlignParam refAlignParam, Base callContext, Category category, Syntax.Base args)
        {
            Result argResult = _definingType.CreateRef(refAlignParam).Conversion(category,_definingType); 
            return VisitApply(callContext,category,args).UseWithArg(argResult);
        }
        /// <summary>
        /// Founds from ref.
        /// </summary>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        public SearchResult FoundFromRef(RefAlignParam refAlignParam)
        {
            return new SearchResultFromRef(this, DefiningType, refAlignParam);
        }
    }

    internal class SearchResultFromRef : SearchResult
    {
        [DumpData(true)]
        private readonly SearchResult _searchResultForTarget;

        private RefAlignParam _refAlignParam;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultFromRef"/> class.
        /// </summary>
        /// <param name="searchResultForTarget">The search result for target.</param>
        /// <param name="target">The target.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        public SearchResultFromRef(SearchResult searchResultForTarget, Type.Base target, RefAlignParam refAlignParam) 
            : base(target.CreateRef(refAlignParam))
        {
            _searchResultForTarget = searchResultForTarget;
            _refAlignParam = refAlignParam;
        }

        /// <summary>
        /// Creates the result for member function searched. Object is provided by use of "Arg" code element
        /// </summary>
        /// <param name="callContext">The call context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public override Result VisitApply(Base callContext, Category category, Syntax.Base args)
        {
            return _searchResultForTarget.VisitApplyFromRef(_refAlignParam, callContext, category, args);
        }
    }
}