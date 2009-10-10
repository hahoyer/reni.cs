using System;
using System.Linq;
using System.Collections.Generic;

namespace Reni.Struct
{
    internal class ContextPosition : ReniObject
    {
        private readonly Context _structContext;
        private readonly int _position;

        internal ContextPosition(Context structContext, int position)
        {
            _position = position;
            _structContext = structContext;
        }

        public PositionFeature ToProperty(bool isProperty) { return new PositionFeature(_structContext, _position, isProperty); }
    }
}