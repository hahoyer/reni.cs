using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;
using Reni.TokenClasses;

namespace Reni.Formatting
{
    sealed class Variants : DumpableObject, Situation.IData
    {
        public static Variants Create
            (
            SourceSyntax ruler,
            Situation.IData multiline,
            Situation.IData singleline,
            int? maxLineLength
            )
            => new Variants(ruler, multiline, singleline, maxLineLength);

        [EnableDump]
        readonly Situation.IData Multiline;
        [EnableDump]
        readonly Situation.IData Singleline;
        readonly SourceSyntax Ruler;
        readonly int? MaxLineLength;

        Variants
            (
            SourceSyntax ruler,
            Situation.IData multiline,
            Situation.IData singleline,
            int? maxLineLength)
        {
            Ruler = ruler;
            Multiline = multiline;
            Singleline = singleline;
            MaxLineLength = maxLineLength;
        }

        [EnableDump]
        string Id => Ruler.NodeDump;

        int Situation.IData.LineCount
            => Multiline.Max < Singleline.Max
                ? Multiline.LineCount
                : Singleline.LineCount;

        int Situation.IData.Max => Math.Min(Multiline.Max, Singleline.Max);

        Situation.IData Situation.IData.Plus(Situation.IData right)
            => Create(Ruler, Multiline.Plus(right), Singleline.Plus(right), MaxLineLength);

        Situation.IData Situation.IData.Plus(int right)
        {
            NotImplementedMethod(right);
            return null;
        }

        bool? Situation.IData.PreferMultiline => PreferMultiline;

        bool PreferMultiline
        {
            get
            {
                if(Multiline.PreferMultiline == true)
                    return true;
                return MaxLineLength != null && Singleline.Max > MaxLineLength.Value;
            }
        }

        Rulers Situation.IData.Rulers
            =>
                PreferMultiline
                    ? Multiline.Rulers.Concat(Ruler, true)
                    : Singleline.Rulers.Concat(Ruler, false);

        Situation.IData Situation.IData.Combine
            (SourceSyntax frame, Situation.IData singleline, Provider formatter)
            => singleline.ReverseCombine(frame, this, formatter);

        Situation.IData Situation.IData.ReversePlus(Lengths left)
            =>
                Create
                    (
                        Ruler,
                        Multiline.ReversePlus(left),
                        Singleline.ReversePlus(left),
                        MaxLineLength);

        Situation.IData Situation.IData.ReverseCombine
            (SourceSyntax frame, Lengths multiline, Provider formatter)
        {
            NotImplementedMethod(frame, multiline, formatter);
            return null;
        }

        Situation.IData Situation.IData.ReverseCombine
            (SourceSyntax frame, Variants multiline, Provider formatter)
            => Create(frame, multiline, this, MaxLineLength);
    }
}