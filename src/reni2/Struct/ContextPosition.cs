using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Struct
{
    internal sealed class ContextPosition : ReniObject
    {
        private readonly SimpleCache<PositionFeature> _propertyFeature;
        private readonly SimpleCache<PositionFeature> _nonPropertyFeature;

        internal ContextPosition(Context structContext, int position)
        {
            _propertyFeature = new SimpleCache<PositionFeature>(() => new PositionFeature(structContext, position, true));
            _nonPropertyFeature = new SimpleCache<PositionFeature>(() => new PositionFeature(structContext, position, false));
        }

        public PositionFeature ToProperty(bool isProperty) { return isProperty ? _propertyFeature.Value : _nonPropertyFeature.Value; }
    }
}