using HWClassLibrary.Debug;
using System;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class PositionFeature : ReniObject, IContextFeature, IConverter<IFeature, Ref>, IFeature
    {
        [DumpData(true)]
        private readonly Context _context;
        [DumpData(true)]
        private readonly int _index;

        public PositionFeature(Context context, int index)
        {
            _context = context;
            _index = index;
        }

        IFeature IConverter<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == _context.RefAlignParam);
            return this;
        }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var objectResult = callContext.ResultAsRef(category | Category.Type, @object).ConvertTo(_context.NaturalRefType);
            return ApplyResult(callContext, category, objectResult, args);
        }

        Result IContextFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax args)
        {
            var objectResult = _context.CreateThisRefResult(category | Category.Type);
            return ApplyResult(callContext, category, objectResult, args);
        }

        Result ApplyResult(ContextBase callContext, Category category, Result objectResult, ICompileSyntax args)
        {
            var trace = false; // category.HasCode | category.HasRefs | category.HasInternal;
            StartMethodDump(trace,callContext,category,objectResult,args);
            var accessResult = _context.NaturalRefType.AccessResult(category|Category.Type, _index).UseWithArg(objectResult);
            if (args == null)
                return ReturnMethodDump(trace,accessResult);
            Dump(trace, "accessResult", accessResult);
            var result = accessResult.Type.ApplyFunction(category, callContext, args);
            return ReturnMethodDumpWithBreak(trace,result);
        }
    }
}