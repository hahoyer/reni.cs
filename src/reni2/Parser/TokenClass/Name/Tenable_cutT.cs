using Reni.Context;
using Reni.Struct;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    internal sealed class Tenable_cutT: Defineable
    {
        sealed internal class SearchResult : Context.SearchResult
        {
            public SearchResult(Type.Base definingType) : base(definingType)
            {
            }

            /// <summary>
            /// Creates the result for member function searched. Object is provided by use of "Arg" code element
            /// </summary>
            /// <param name="callContext">The call context.</param>
            /// <param name="category">The category.</param>
            /// <param name="args">The args.</param>
            /// <returns></returns>
            protected internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
            {
                if (args == null)
                    return DefiningType.CreateEnableCut().CreateArgResult(category);
                NotImplementedMethod(callContext, category, args);
                return null;
            }
        }

        internal override Context.SearchResult Search(Sequence sequence)
        {
            return new SearchResult(sequence);
        }

        internal override Reni.StructContainerSearchResult SearchFromStruct()
        {
            return new StructContainerSearchResult();
        }

        sealed internal class StructContainerSearchResult : Reni.StructContainerSearchResult
        {
            internal override Result Visit(Struct.Type definingType, Context.Base callContext, Category category,
                                           Syntax.Base args)
            {
                if (args == null)
                    return definingType.CreateArgResult(category);
                NotImplementedMethod(callContext, category, args);
                return null;
            }
        }
    }
}