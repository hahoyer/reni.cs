using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Situation : DumpableObject
    {
        internal interface IData
        {
            IData Add(int right);
            IData Add(IData right);
            IData ReverseAdd(Lengths left);
            IData Combine(Frame frame, IData singleline);
            IData ReverseCombine(Frame frame, Lengths multiline);
            IData ReverseCombine(Frame frame, Variants multiline);
            int LineCount { get; }
            int Max { get; }
            bool? PreferMultiline { get; }
            Rulers Rulers { get; }
        }

        public static Situation Empty => new Situation();

        public static Situation Create(IEnumerable<int> lengths)
            => new Situation(lengths);

        public static Situation operator +(Situation left, int right) => left.Add(right);

        public static Situation operator +(Situation left, Situation right) => left.Add(right);

        [EnableDump]
        readonly IData _data;

        Situation(IEnumerable<int> lengths = null)
            : this(new Lengths(lengths)) {}

        Situation(IData data) { _data = data; }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "[" + Max + "*" + LineCount + "]";

        Situation Add(int right) => new Situation(_data.Add(right));
        Situation Add(Situation right) => new Situation(_data.Add(right._data));

        internal int Max => _data.Max;
        internal int LineCount => _data.LineCount;

        internal Situation Combine(Frame frame, Situation singleLine)
            => new Situation(_data.Combine(frame, singleLine._data));

        internal Rulers Rulers => _data.Rulers;
    }
}