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

        private List<ICompileSyntax> StatementList { get { return Context.StatementList; } }

        internal override Size Size { get { return InternalResult(Category.Size).Size; } }

        internal Result ConstructorResult(Category category)
        {
            return CreateResult(category, InternalResult(category - Category.Type));
        }

        private Result InternalResult(Category category)
        {
            return Context.InternalResult(category);
        }

        private Result AccessResult(Category category, int i)
        {
            return Context.AccessResult(category, i);
        }

        internal override Result DumpPrintFromRef(Category category, RefAlignParam refAlignParam)
        {
            Tracer.Assert(refAlignParam.Equals(Context.RefAlignParam));
            var result = new List<Result>();
            for (var i = 0; i < StatementList.Count; i++)
            {
                var accessResult = AccessResult(category|Category.Type, i);
                result.Add(accessResult.Type.DumpPrint(category).UseWithArg(accessResult));
            }
            return Result.ConcatPrintResult(category, result);
        }
    }
}