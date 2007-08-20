using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    sealed class TtypeT: Defineable
    {
        /// <summary>
        /// Class for result handler for structures
        /// </summary>
        sealed class FoundResult : SearchResult
        {
            public FoundResult(Type.Base obj)
                : base(obj)
            {
            }

            /// <summary>
            /// Obtain result
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="category">The category.</param>
            /// <param name="obj">The obj.</param>
            /// <param name="args">The args.</param>
            /// <returns></returns>
            public override Result VisitApply(Context.Base context, Category category, Syntax.Base args)
            {
                if (args == null)
                    return DefiningType.TypeOperator(category);
                Result argResult = args.Visit(context, category | Category.Type);
                return DefiningType.ApplyTypeOperator(argResult);
            }
        }


        /// <summary>
        /// Gets the type operation.
        /// </summary>
        /// <value>The type operation.</value>
        /// created 07.01.2007 16:24
        public override SearchResult DefaultOperation(Type.Base obj)
        {
            return new FoundResult(obj);
        }

    }
}
