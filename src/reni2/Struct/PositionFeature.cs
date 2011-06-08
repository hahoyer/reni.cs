using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Context;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class PositionFeature :
        ReniObject,
        IContextFeature,
        IFeature
    {
        [IsDumpEnabled(true)]
        private readonly Container _container;

        [IsDumpEnabled(true)]
        private readonly ContextBase _parent;

        [IsDumpEnabled(true)]
        private readonly int _position;

        [IsDumpEnabled(true)]
        private readonly bool _isProperty;

        internal PositionFeature(Container container, ContextBase parent, int position, bool isProperty)
        {
            _container = container;
            _parent = parent;
            _position = position;
            _isProperty = isProperty;
        }

        TypeBase IFeature.DefiningType() { return _structContext.ContextReferenceType; }

        Result IFeature.Apply(Category category) { return _structContext.PositionFeatureApply(category, _position, _isProperty, false); }
        Result IContextFeature.Apply(Category category) { return _structContext.PositionFeatureApply(category, _position, _isProperty, true); }
    }
}