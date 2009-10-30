// #pragma warning disable 649
using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Code;
using Reni.Context;
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
            Tracer.Assert(type.RefAlignParam == RefAlignParam);
            return this;
        }

        Result IFeature.Apply(Category category)
        {
            return Apply(category);
        }

        private RefAlignParam RefAlignParam { get { return _structContext.ForCode.RefAlignParam; } }

        private Result Apply(Category category)
        {
            var result = _structContext.AccessResultAsContextRef(category | Category.Type, _position);
            if(!_isProperty)
                return result;
            return result
                .Type
                .GetFunctionalFeature()
                .Apply(category, result, TypeBase.CreateVoidResult(category | Category.Type));
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        Result IContextFeature.Apply(Category category)
        {
            return Apply(category);
        }
    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
    }
}