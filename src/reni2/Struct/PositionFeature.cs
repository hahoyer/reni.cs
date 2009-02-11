using HWClassLibrary.Debug;
using System;
using Reni.Code;
using Reni.Context;
using Reni.Feature;
using Reni.Syntax;
using Reni.Type;

namespace Reni.Struct
{
    internal abstract class PositionFeatureBase : ReniObject, IContextFeature, IConverter<IFeature, Ref>, IFeature
    {
        private readonly IStructContext _structContext;

        [DumpData(true)]
        private readonly int _index;

        protected PositionFeatureBase(IStructContext structContext, int index)
        {
            _index = index;
            _structContext = structContext;
        }

        IFeature IConverter<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == _structContext.ForCode.RefAlignParam);
            return this;
        }

        [DumpData(false)]
        private Ref NaturalRefType { get { return _structContext.NaturalRefType; } }

        Result IContextFeature.ApplyResult(ContextBase callContext, Category category, ICompileSyntax args)
        {
            return ApplyResult(callContext, category, null, args);
        }

        public virtual Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                     ICompileSyntax args)
        {
            var trace = ObjectId == 1541 && callContext.ObjectId == 5 && (category.HasCode);
            StartMethodDumpWithBreak(trace, callContext, category, @object, args);
            var accessResult = NaturalRefType.AccessResult(category | Category.Type, _index)
                .UseWithArg(objectResult);
            var rawResult = accessResult;
            if (args != null)
            {
                Dump(trace, "accessResult", accessResult);
                rawResult = accessResult.Type.ApplyFunction(category, callContext, args);
            }

            var objectResult = ObjectResult(callContext, category, @object);
            var result = rawResult.ReplaceRelativeContextRef()
            return ReturnMethodDumpWithBreak(trace, rawResult);
        }

        private Result ObjectResult(ContextBase callContext, Category category, ICompileSyntax @object)
        {
            if(@object == null)
                return NaturalRefType.CreateContextResult(_structContext.ForCode, category | Category.Type);
            return callContext.ResultAsRef(category | Category.Type, @object).ConvertTo(NaturalRefType);
        }

    }

    [Serializable]
    internal sealed class PositionFeature : PositionFeatureBase
    {
        private readonly PropertyPositionFeature _property;

        internal PositionFeature(EmptyList emptyList, IStructContext structContext, int index)
            : base(structContext, index)
        {
            _property = new PropertyPositionFeature(emptyList, structContext, index);
        }

        public PositionFeatureBase ToProperty(bool isPoperty)
        {
            if(isPoperty)
                return _property;
            return this;
        }
    }

    internal class PropertyPositionFeature : PositionFeatureBase
    {
        private readonly EmptyList _emptyList;

        public PropertyPositionFeature(EmptyList emptyList, IStructContext structContext, int index)
            : base(structContext, index)
        {
            _emptyList = emptyList;
        }

        public override Result ApplyResult(ContextBase callContext, Category category, ICompileSyntax @object,
                                           ICompileSyntax args)
        {
            var result = base.ApplyResult(callContext, category, @object, _emptyList);
            if (args == null)
                return result;
            NotImplementedMethod(callContext, category, @object, args);
            return null;
        }
    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
    }
}