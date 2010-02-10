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
            return Apply(category)
                .ReplaceRelativeContextRef(_structContext.ForCode, _structContext.ContextRefCodeAsArgCode());
        }

        private RefAlignParam RefAlignParam { get { return _structContext.ForCode.RefAlignParam; } }

        private Result Apply(Category category)
        {
            NotImplementedMethod(category);
            return null;
        }

        TypeBase IFeature.DefiningType() { return _structContext.NaturalRefType; }

        Result IContextFeature.Apply(Category category) { return Apply(category); }
    }

    internal interface IStructContext
    {
        Ref NaturalRefType { get; }
        IRefInCode ForCode { get; }
        ThisType GetThisType();
    }

    internal class ThisType: TypeBase
    {
        protected override Size GetSize() { throw new NotImplementedException(); }

        internal override string DumpShort() { throw new NotImplementedException(); }

        internal override bool IsValidRefTarget() { throw new NotImplementedException(); }

        internal Result CreateContextResult(IRefInCode context, Category category)
        {
            return CreateResult(
                category,
                () => CodeBase.CreateContextRef(context).CreateRefPlus(context.RefAlignParam, Parent.Size * -1),
                () => Refs.Context(context));
        }

    }
}