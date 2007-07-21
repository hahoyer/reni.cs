using System;
using HWClassLibrary.Debug;
using Reni.Context;

namespace Reni.Parser.TokenClass
{
    /// <summary>
    /// Search result object for numeric operations (BitArrayOperation)
    /// </summary>
    public sealed class FoundNumpopResult : SearchResult
    {
        [DumpData(true)]
        private readonly Defineable _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FoundNumpopResult"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="obj">The obj.</param>
        /// created 08.01.2007 01:58
        public FoundNumpopResult(Defineable parent, Type.Base obj)
            : base(obj)
        {
            _parent = parent;
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
            bool trace = ObjectId == -296 && context.ObjectId == 5 && category.HasCode;
            StartMethodDumpWithBreak(trace, context, category, args);
            Type.Base set = DefiningType.SequenceElementType;
            Result objResult = DefiningType.VisitAsSequence(category, set);
            Result argResult = args.VisitAsSequence(context, category | Category.Type, set);
            if (trace) DumpMethodWithBreak("",context, category, args,"objResult",objResult,"argResult",argResult);
            Result result = new Result();
            if (category.HasSize || category.HasType || category.HasCode)
            {
                int objBitCount = DefiningType.UnrefSize.ToInt();
                int argBitCount = argResult.Type.UnrefSize.ToInt();
                Type.Base type = set.OperationResultType(_parent, objBitCount, argBitCount).CreateAlign(context.RefAlignParam.AlignBits);
                if (category.HasSize) result.Size = type.Size;
                if (category.HasType) result.Type = type;
                if (category.HasCode) result.Code = set.CreateOperation(_parent, objResult, type.Size, argResult);
            };
            if (category.HasRefs) result.Refs = objResult.Refs.Pair(argResult.Refs);
            return ReturnMethodDumpWithBreak(trace, result);
        }

    }
    internal class FoundNumericPrefixOperationResult : PrefixSearchResult
    {
        [DumpData(true)]
        private readonly Defineable _parent;

        public FoundNumericPrefixOperationResult(Defineable parent)
        {
            _parent = parent;
        }
        /// <summary>
        /// Visits the prefix apply.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// created 02.02.2007 21:54
        public override Result VisitApply(Category category, Result obj)
        {
            Type.Base set = obj.Type.SequenceElementType;
            Result objResult = obj.Type.VisitAsSequence(category, set).UseWithArg(obj);
            Result result = new Result();
            if (category.HasSize || category.HasType || category.HasCode)
            {
                if (category.HasSize) result.Size = objResult.Size;
                if (category.HasType) result.Type = objResult.Type;
                if (category.HasCode) result.Code = set.CreateOperation(_parent, objResult);
            };
            if (category.HasRefs) result.Refs = objResult.Refs;
            return result;
        }

    }
}