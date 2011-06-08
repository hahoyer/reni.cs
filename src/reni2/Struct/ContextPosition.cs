using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Context;

namespace Reni.Struct
{
    internal sealed class ContextPosition : ReniObject
    {
        private readonly SimpleCache<PositionFeature> _propertyFeature;
        private readonly SimpleCache<PositionFeature> _nonPropertyFeature;

        internal ContextPosition(Container container, ContextBase parent, int position)
        {
            _propertyFeature = new SimpleCache<PositionFeature>(() => new PositionFeature(container, parent, position, true));
            _nonPropertyFeature = new SimpleCache<PositionFeature>(() => new PositionFeature(container, parent, position, false));
        }

        public PositionFeature ToProperty(bool isProperty) { return isProperty ? _propertyFeature.Value : _nonPropertyFeature.Value; }
    }
}