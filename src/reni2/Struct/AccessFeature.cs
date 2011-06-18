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
        private readonly AccessManager.IAccessObject _accessObject;

        [EnableDump]
        private readonly AccessPoint _accessPoint;

        [EnableDump]
        private readonly int _position;

        internal AccessFeature(AccessManager.IAccessObject accessObject, AccessPoint accessPoint, int position)
        {
            _accessObject = accessObject;
            _accessPoint = accessPoint;
            _position = position;
        }

        TypeBase IFeature.DefiningType() { return _accessPoint.Type; }

        Result IFeature.Apply(Category category) { return _accessObject.Access(category, _accessPoint, _position, false); }
        Result IContextFeature.Apply(Category category) { return _accessObject.Access(category, _accessPoint, _position, true); }
    }
}