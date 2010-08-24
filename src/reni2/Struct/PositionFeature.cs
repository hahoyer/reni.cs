using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    internal class PositionFeature :
        ReniObject,
        IContextFeature,
        IFeature
    {
        [IsDumpEnabled(true)]
        private readonly Context _structContext;

        [IsDumpEnabled(true)]
        private readonly int _position;

        [IsDumpEnabled(true)]
        private readonly bool _isProperty;

        public PositionFeature(Context structContext, int position, bool isProperty)
        {
            _isProperty = isProperty;
            _structContext = structContext;
            _position = position;
        }

        TypeBase IFeature.DefiningType() { return _structContext.ContextReferenceType; }

        Result IFeature.Apply(Category category) { return _structContext.PositionFeatureApply(category, _position, _isProperty, false); }
        Result IContextFeature.Apply(Category category) { return _structContext.PositionFeatureApply(category, _position, _isProperty, true); }
    }
}