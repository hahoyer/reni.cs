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

        Result IFeature.Apply(Category category)
        {
            return Apply(category)
                .ReplaceRelativeContextRef(_structContext.ForCode, ()=>_structContext.ContextRefCodeAsArgCode());
        }

        private RefAlignParam RefAlignParam { get { return _structContext.ForCode.RefAlignParam; } }

        private Result Apply(Category category)
        {
            return _structContext.CreateAtResultFromContext(category, _position);
        }

        TypeBase IFeature.DefiningType() { return _structContext.ContextType; }

        Result IContextFeature.Apply(Category category) { return Apply(category); }
    }
}