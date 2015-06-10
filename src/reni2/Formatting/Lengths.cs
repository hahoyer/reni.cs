using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.TokenClasses;

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
            //Tracer.ConditionalBreak(LineCount == 4 && _lengths[2] == 9 && _lengths[3] == 9, Dump);
        }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "[" + Max + "*" + LineCount + "]";

        int Situation.IData.Max => Max;
        int Situation.IData.LineCount => LineCount;

        Situation.IData Situation.IData.Plus(int right)
            => Plus(new[] {right});

        Situation.IData Situation.IData.Plus(Situation.IData right)
            => IsEmpty ? right : right.ReversePlus(this);

        Situation.IData Situation.IData.ReversePlus(Lengths left)
            => IsEmpty ? left : left.Plus(this);

        Situation.IData Situation.IData.Combine(SourceSyntax frame, Situation.IData singleline, Provider formatter)
            => singleline.ReverseCombine(frame, this,formatter);

        Situation.IData Situation.IData.ReverseCombine(SourceSyntax frame, Lengths multiline, Provider formatter)
        {
            if(formatter.MinImprovementOfLineBreak > Max - multiline.Max)
                return this;
            return Variants.Create(frame, multiline, this, formatter.MaxLineLength);
        }

        Situation.IData Situation.IData.ReverseCombine(SourceSyntax frame, Variants multiline, Provider formatter)
        {
            NotImplementedMethod(frame, multiline);
            return null;
        }

        bool? Situation.IData.PreferMultiline => null;
        Rulers Situation.IData.Rulers => Rulers.Empty;

        bool IsEmpty => LineCount == 0 && Max == 0;
        int Max => _lengths.Max();
        int LineCount => _lengths.Length - 1;


        Situation.IData Plus(Lengths right) => Plus(right._lengths);
        Situation.IData Plus(IEnumerable<int> right) => new Lengths(OverlappingAdd(_lengths, right));
    }
}