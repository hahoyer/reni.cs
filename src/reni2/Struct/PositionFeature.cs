using HWClassLibrary.Debug;
using System;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    [Serializable]
    internal sealed class PositionFeature : ReniObject, IContextFeature, IConverter<IFeature, Ref>, IFeature
    {
        private readonly IStructContext _structContext;
        [DumpData(true)]
        private readonly int _index;

        [DumpData(false)]
        internal Ref NaturalRefType { get { return _structContext.NaturalRefType; } }

        internal PositionFeature(IStructContext structContext, int index)
        {
            _index = index;
            _structContext = structContext;
        }

        IFeature IConverter<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == _structContext.ForCode.RefAlignParam);
            return this;
        }

        Result IFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object, ICompileSyntax args)
        {
            var objectResult = callContext.ResultAsRef(category | Category.Type, @object).ConvertTo(NaturalRefType);
            return ApplyResult(callContext, category, objectResult, args);
        }

        Result IContextFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax args)
        {
            var objectResult = NaturalRefType.CreateContextResult(_structContext.ForCode, category | Category.Type);
            return ApplyResult(callContext, category, objectResult, args);
        }

        Result ApplyResult(ContextBase callContext, Category category, Result objectResult, ICompileSyntax args)
        {
            var trace = ObjectId==222 && category.HasCode && category.HasRefs;
            StartMethodDumpWithBreak(trace,callContext,category,objectResult,args);
            var accessResult = NaturalRefType.AccessResult(category|Category.Type, _index).UseWithArg(objectResult);
            if (args == null)
                return ReturnMethodDump(trace,accessResult);
            Dump(trace, "accessResult", accessResult);
            var result = accessResult.Type.ApplyFunction(category, callContext, args);
            return ReturnMethodDumpWithBreak(trace,result);
        }
    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
    }
}