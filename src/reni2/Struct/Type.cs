using System;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class Type : TypeBase
    {
        internal readonly Context Context;

        public Type(Context context)
        {
            Context = context;
        }

        internal override string DumpShort()
        {
            return "type." + ObjectId + "(context." + Context.ObjectId + ")";
        }

        [DumpData(false)]
        internal protected override int IndexSize { get { return Context.IndexSize; } }

        [DumpData(false)]
        internal override Size Size { get { return InternalResult(Category.Size).Size; } }

        internal Result ConstructorResult(Category category)
        {
            return CreateResult(category, InternalResult(category - Category.Type));
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam.Equals(Context.RefAlignParam));
            var result = new List<Result>();
            for (var i = 0; i < StatementList.Count; i++)
            {
                var accessResult = AccessResult(category | Category.Type, i);
                result.Add(accessResult.Type.DumpPrint(category).UseWithArg(accessResult));
            }
            return Result.ConcatPrintResult(category, result);
        }

        private List<ICompileSyntax> StatementList { get { return Context.StatementList; } }

        private Result InternalResult(Category category)
        {
            return Context.InternalResult(category);
        }

        internal Result AccessResult(Category category, int i)
        {
            return Context.AccessResult(category, i);
        }
    }
}