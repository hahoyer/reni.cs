using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Lengths : DumpableObject, Situation.IData
    {
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

        [EnableDump]
        readonly int[] _lengths;

        internal Lengths(IEnumerable<int> lengths = null)
        {
            _lengths = lengths?.ToArray() ?? new[] {0};
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "[" + Max + "*" + LineCount + "]";

        int Situation.IData.Max => Max;
        int Situation.IData.LineCount => LineCount;

        Situation.IData Situation.IData.Add(int right) 
            => Add(new[] {right});

        Situation.IData Situation.IData.Add(Situation.IData right)
            => IsEmpty ? right : right.ReverseAdd(this);

        Situation.IData Situation.IData.ReverseAdd(Lengths left) 
            => IsEmpty ? left : left.Add(this);

        Situation.IData Situation.IData.Combine(Frame frame, Situation.IData singleline)
            => singleline.ReverseCombine(frame, this);

        Situation.IData Situation.IData.ReverseCombine(Frame frame, Lengths multiline)
        {
            if(frame.Formatter.MinImprovementOfLineBreak > Max - multiline.Max)
                return this;
            return Variants.Create(frame, multiline, this);
        }

        Situation.IData Situation.IData.ReverseCombine(Frame frame, Variants multiline)
        {
            NotImplementedMethod(frame, multiline);
            return null;
        }

        bool? Situation.IData.PreferMultiline => null;
        Rulers Situation.IData.Rulers => Rulers.Empty;

        bool IsEmpty => LineCount == 0 && Max == 0;
        int Max => _lengths.Max();
        int LineCount => _lengths.Length - 1;


        Situation.IData Add(Lengths right) => Add(right._lengths);
        Situation.IData Add(IEnumerable<int> right) => new Lengths(OverlappingAdd(_lengths, right));
    }
}