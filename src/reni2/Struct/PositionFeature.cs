using System;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    internal class PositionFeature :
        ReniObject,
        ISearchPath<IFeature, Ref>,
        IContextFeature,
        IFeature
    {
        [DumpData(true)]
        private readonly Context _structContext;
        [DumpData(true)]
        private readonly int _position;
        [DumpData(true)]
        private readonly bool _isProperty;

        public PositionFeature(Context structContext, int position, bool isProperty)
        {
            _isProperty = isProperty;
            _structContext = structContext;
            _position = position;
        }

        IFeature ISearchPath<IFeature, Ref>.Convert(Ref type)
        {
            Tracer.Assert(type.RefAlignParam == _structContext.ForCode.RefAlignParam);
            return this;
        }

        Result IFeature.Apply(Category category, TypeBase objectType)
        {
            return ApplyProperty(category, objectType.AccessResultAsArg(category | Category.Type, _position));
        }

        Result IContextFeature.Apply(Category category)
        {
            return ApplyProperty(category, _structContext.AccessResultAsContextRef(category | Category.Type, _position));
        }

        private Result ApplyProperty(Category category, Result applyResult)
        {
            if (!_isProperty)
                return applyResult;

            return applyResult
                .Type
                .GetFunctionalFeature()
                .Apply(category, applyResult, TypeBase.CreateVoidResult(category | Category.Type));
        }

    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
    }
}