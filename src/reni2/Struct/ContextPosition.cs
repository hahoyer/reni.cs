using System;
using System.Linq;
using System.Collections.Generic;
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;

namespace Reni.Struct
{
    internal class ContextPosition : ReniObject
    {
        private readonly SimpleCache<PositionFeature> _trueFeature;
        private readonly SimpleCache<PositionFeature> _falseFeature;

        internal ContextPosition(Context structContext, int position)
        {
            _trueFeature = new SimpleCache<PositionFeature>(() => new PositionFeature(structContext, position, true));
            _falseFeature = new SimpleCache<PositionFeature>(() => new PositionFeature(structContext, position, false));
        }

        public PositionFeature ToProperty(bool isProperty) { return isProperty ? _trueFeature.Value : _falseFeature.Value; }
    }
}