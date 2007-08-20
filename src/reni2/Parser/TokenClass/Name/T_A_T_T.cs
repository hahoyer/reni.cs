using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass.Name
{
    sealed class T_A_T_T: Defineable
    {
        /// <summary>
        /// Class for result handler for structures
        /// </summary>
        sealed class FoundResult : StructSearchResult
        {
            [DumpData(true)]
            private readonly T_A_T_T _parent;

            public FoundResult(T_A_T_T parent, Context.Struct @struct) : base(@struct)
            {
                _parent = parent;
            }

            /// <summary>
            /// Obtain result
            /// </summary>
            /// <param name="callContext">The callContext.</param>
            /// <param name="category">The category.</param>
            /// <param name="args">The args.</param>
            /// <returns></returns>
            /// created 21.05.2007 23:41 on HAHOYER-DELL by hh
            public override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
            {
                return VisitATApply(callContext, category, args);
            }
        }

        /// <summary>
        /// Gets the struct operation.
        /// </summary>
        /// <param name="struc">The struc.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <value>The struct operation.</value>
        /// [created 14.05.2006 22:23]
        internal override bool IsStructOperation { get { return true; } }
    }
}
