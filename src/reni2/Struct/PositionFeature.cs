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
        [EnableDump]
        private readonly PositionContainerContext _context;

        [EnableDump]
        private readonly bool _isProperty;

        internal PositionFeature(PositionContainerContext context, bool isProperty)
        {
            _context = context;
            _isProperty = isProperty;
        }

        TypeBase IFeature.DefiningType() { return _context.ContextReferenceType; }

        Result IFeature.Apply(Category category) { return _context.PositionFeatureApply(category, _isProperty, false); }
        Result IContextFeature.Apply(Category category) { return _context.PositionFeatureApply(category, _isProperty, true); }
    }
}