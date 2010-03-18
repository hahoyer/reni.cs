using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
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
                .ReplaceRelativeContextRef(_structContext.ForCode, ()=>_structContext.ContextRefCodeAsArgCode());
        }

        private RefAlignParam RefAlignParam { get { return _structContext.ForCode.RefAlignParam; } }

        private Result Apply(Category category)
        {
            return _structContext.CreateAccess(_position).CreateResult(category, _structContext.CreateContextCode, _structContext.CreateContextRefs);
        }

        TypeBase IFeature.DefiningType() { return _structContext.ThisType; }

        Result IContextFeature.Apply(Category category) { return Apply(category); }
    }
}