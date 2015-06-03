using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Lengths : DumpableObject, Situation.IData
    {
        [EnableDump]
        readonly int[] _lengths;

        internal Lengths(IEnumerable<int> lengths = null)
        {
            _lengths = lengths?.ToArray() ?? new[] {0};
            StopByObjectIds();
            Tracer.ConditionalBreak(Max > 100);
        }

        Situation.IData Situation.IData.Add(int right) => Add(new[] {right});
        Situation.IData Situation.IData.Add(Situation.IData right) => right.ReverseAdd(this);
        Situation.IData Situation.IData.ReverseAdd(Lengths left) => left.Add(this);

        Situation.IData Situation.IData.Combine(Frame frame, Situation.IData singleline)
            => singleline.ReverseCombine(frame, this);

        Situation.IData Situation.IData.ReverseCombine(Frame frame, Lengths multiline)
        {
            if(frame.Formatter.MinImprovementOfLineBreak > Max - multiline.Max)
                return this;
            return new Variants(frame, multiline, this);
        }

        Situation.IData Add(Lengths right) => Add(right._lengths);
        Situation.IData Add(IEnumerable<int> right) => new Lengths(OverlappingAdd(_lengths, right));

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