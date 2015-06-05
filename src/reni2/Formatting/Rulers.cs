using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Rulers : DumpableObject
    {
        internal static readonly Rulers Empty = new Rulers();

        readonly Dictionary<Frame, bool> _data;

        Rulers() { _data = new Dictionary<Frame, bool>(); }

        Rulers(Dictionary<Frame, bool> data, Frame other, bool isMultiline)
        {
            _data = data.ToDictionary(item => item.Key, item => item.Value);
            _data[other] = isMultiline;
        }

        public Rulers Concat(Frame other, bool isMultiline)
            => new Rulers(_data, other, isMultiline);

        public bool IsMultiLine(Frame lineBreakRuler)
        {
            bool result;
            return _data.TryGetValue(lineBreakRuler, out result) && result;
        }
    }
}