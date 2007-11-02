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
        protected abstract Result VisitApply(Base callContext, Category category, Syntax.Base args);

        internal virtual Result VisitApplyFromRef(Base callContext, Category category, Syntax.Base args, Ref refType)
        {
            Result result = VisitApply(callContext, category, args);
            result = result.UseWithArg(refType.Conversion(category,_definingType));
            return result;
        }

        public SearchResultFromRef FromRef()
        {
            NotImplementedMethod();
            return null;
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

