using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Option : DumpableObject
    {
        [EnableDump]
        readonly int[] _lengths;

        internal Option(IEnumerable<int> lengths = null)
        {
            _lengths = lengths?.ToArray() ?? new[] {0}; // There is always one line
        }

        internal Option Add(Option right) => Add(right._lengths);
        internal Option Add(int right) => Add(new[] {right});

        Option Add(IEnumerable<int> right) => new Option(OverlappingAdd(_lengths, right));

        static IEnumerable<int> OverlappingAdd(IEnumerable<int> left, IEnumerable<int> right)
        {
            int? resultItem = null;

            foreach(var item in left)
            {
                if(resultItem != null)
                    yield return resultItem.Value;
                resultItem = item;
            }

            foreach(var item in right)
            {
                yield return item + (resultItem ?? 0);
                resultItem = null;
            }
        }

        public int Max => _lengths.Max();

    }
}