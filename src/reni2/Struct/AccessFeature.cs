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
        ISuffixFeature
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

        TypeBase IFeature.ObjectType { get { return _structure.Type; } }

        Result IFeature.Result(Category category, RefAlignParam refAlignParam) { return _structure.AccessViaThisReference(category, _position); }
        Result IContextFeature.ObtainResult(Category category) { return _structure.AccessViaContextReference(category, _position); }
    }
}