using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Feature;
using Reni.Type;

namespace Reni.Struct
{
    internal sealed class AccessFeature :
        ReniObject,
        IContextFeature,
        IFeature
    {
        [EnableDump]
        private readonly AccessPoint _accessPoint;

        [EnableDump]
        private readonly int _position;

        [EnableDump]
        private readonly bool _isProperty;

        internal AccessFeature(AccessPoint accessPoint, int position, bool isProperty)
        {
            _accessPoint = accessPoint;
            _isProperty = isProperty;
            _position = position;
        }

        TypeBase IFeature.DefiningType()
        {
            NotImplementedMethod();
            return null;
        }

        Result IFeature.Apply(Category category) { return Apply(category, false); }
        Result IContextFeature.Apply(Category category) { return Apply(category, true); }

        private Result Apply(Category category, bool isContextFeature) { return _accessPoint.Access(category, _position, _isProperty, isContextFeature); }

    }
}