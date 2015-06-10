using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Situation : DumpableObject
    {
        internal interface IData
        {
            IData Plus(int right);
            IData Plus(IData right);
            IData ReversePlus(Lengths left);
            IData Combine(SourceSyntax frame, IData singleline, Provider formatter);
            IData ReverseCombine(SourceSyntax frame, Lengths multiline, Provider formatter);
            IData ReverseCombine(SourceSyntax frame, Variants multiline, Provider formatter);
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

        Situation Add(int right) => new Situation(_data.Plus(right));
        Situation Add(Situation right) => new Situation(_data.Plus(right._data));

        internal int Max => _data.Max;
        internal int LineCount => _data.LineCount;

        internal Situation Combine(SourceSyntax frame, Situation singleLine, Provider formatter)
            => new Situation(_data.Combine(frame, singleLine._data,formatter));

        internal Rulers Rulers => _data.Rulers;
    }
}