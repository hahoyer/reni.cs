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

        internal AccessFeature(AccessPoint accessPoint, int position)
        {
            _accessPoint = accessPoint;
            _position = position;
        }

        TypeBase IFeature.DefiningType() { return _accessPoint.Type; }

        Result IFeature.Apply(Category category) { return _accessPoint.AccessViaThisReference(category, _position); }
        Result IContextFeature.Apply(Category category) { return _accessPoint.AccessViaContextReference(category, _position); }
    }
}