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
            IData Combine(SourceSyntax ruler, IData singleline, Provider formatter);
            IData ReverseCombine(SourceSyntax ruler, Lengths multiline, Provider formatter);
            IData ReverseCombine(SourceSyntax ruler, Variants multiline, Provider formatter);
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
        readonly IData Data;

        Situation(IEnumerable<int> lengths = null)
            : this(new Lengths(lengths)) { }

        Situation(IData data) { Data = data; }

        protected override string GetNodeDump()
            => base.GetNodeDump() + "[" + Max + "*" + LineCount + "]";

        Situation Add(int right) => new Situation(Data.Plus(right));
        Situation Add(Situation right) => new Situation(Data.Plus(right.Data));

        internal int Max => Data.Max;
        internal int LineCount => Data.LineCount;

        internal Situation Combine(SourceSyntax ruler, Situation singleLine, Provider formatter)
            => new Situation(Data.Combine(ruler, singleLine.Data, formatter));

        internal Rulers Rulers => Data.Rulers;
    }
}