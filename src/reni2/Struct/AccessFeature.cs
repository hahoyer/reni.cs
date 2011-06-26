using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
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
        private readonly Structure _structure;

        [EnableDump]
        private readonly int _position;

        internal AccessFeature(Structure structure, int position)
        {
            _structure = structure;
            _position = position;
        }

        TypeBase IFeature.DefiningType() { return _structure.Type; }

        Result IFeature.Apply(Category category, RefAlignParam refAlignParam) { return _structure.AccessViaThisReference(category, _position); }
        Result IContextFeature.Apply(Category category) { return _structure.AccessViaContextReference(category, _position); }
    }
}