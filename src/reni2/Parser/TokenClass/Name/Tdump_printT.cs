using System;
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

        internal override SearchResultFromRef SearchResultFromRef
        {
            get { return new DumpPrintSearchResult(); }
        }

    }

    internal class DumpPrintSearchResult : SearchResultFromRef
    {
        internal override Result VisitApply(Context.Base callContext, Category category, Syntax.Base args, Ref definingType)
        {
            if (args != null)
                NotImplementedMethod(callContext, category, args);
            bool trace =
                ObjectId == 184
                && callContext.ObjectId == 0
                && category.ToString() == "Size,Type,Refs,Code"
                ;
            StartMethodDump(trace, callContext, category, args);
            Result result = definingType.Target.DumpPrintFromRef(category, definingType.RefAlignParam);
            return ReturnMethodDumpWithBreak(trace, result);
        }
    }
}
