using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Type;

namespace Reni.Parser.TokenClass.Name
{
    /// <summary>
    /// Summary description for printnumToken.
    /// </summary>
    sealed class Tdump_printT : Defineable
    {
        /// <summary>
        /// Gets the type operation.
        /// </summary>
        /// <value>The type operation.</value>
        /// created 07.01.2007 16:24
        internal override bool IsDefaultOperation { get { return true; } }
        /// <summary>
        /// Gets the type operation.
        /// </summary>
        /// <value>The type operation.</value>
        /// created 07.01.2007 16:24
        internal override bool IsRefOperation { get { return true; } }

        /// <summary>
        /// Type.of result of numeric operation, i. e. obj and arg are of type bit array
        /// </summary>
        /// <param name="objSize">not used </param>
        /// <param name="argSize">not used </param>
        /// <returns></returns>
        /// created 08.01.2007 01:40
        internal override Type.Base BitSequenceOperationResultType(int objSize, int argSize)
        {
            return Type.Base.CreateVoid;
        }
        /// <summary>
        /// Search result object for numeric operations (BitArrayOperation)
        /// </summary>
        public sealed class FoundDumpPrintResult : SearchResult
        {
            [DumpData(true)]
            public FoundDumpPrintResult(Type.Base obj):base(obj)                                                                  
            {
            }

            /// <summary>
            /// Obtain result
            /// </summary>
            /// <param name="callContext">The call context.</param>
            /// <param name="category">The category.</param>
            /// <param name="args">The args.</param>
            /// <returns></returns>
            public override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args)
            {
                NotImplementedMethod(callContext, category, args);
                return null;
            }

        }
        /// <summary>
        /// Creates the result for member function searched. Object is provided as reference by use of "Arg" code element
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="category">The category.</param>
        /// <param name="args">The args.</param>
        /// <param name="refAlignParam">The ref align param.</param>
        /// <returns></returns>
        override internal Result VisitRefOperationApply(Context.Base context, Category category, Syntax.Base args, Ref definingType)
        {
            if (args != null)
                NotImplementedMethod(context, category, args);
            bool trace =
                ObjectId == 184
                && context.ObjectId == 0
                && category.ToString() == "Size,Type,Refs,Code"
                ;
            StartMethodDump(trace, context, category, args);
            Result result = definingType.Target.DumpPrintFromRef(category, definingType.RefAlignParam);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}
