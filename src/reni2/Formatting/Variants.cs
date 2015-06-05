using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Variants : DumpableObject, Situation.IData
    {
        public static Variants Create
            (Frame frame, Situation.IData multiline, Situation.IData singleline)
            => new Variants(frame, multiline, singleline);

        [EnableDump]
        readonly Situation.IData _multiline;
        [EnableDump]
        readonly Situation.IData _singleline;
        readonly Frame _frame;

        Variants(Frame frame, Situation.IData multiline, Situation.IData singleline)
        {
            _frame = frame;
            _multiline = multiline;
            _singleline = singleline;
        }

        [EnableDump]
        string Id => _frame.NodeDump;

        int Situation.IData.LineCount
            => _multiline.Max < _singleline.Max
                ? _multiline.LineCount
                : _singleline.LineCount;

        int Situation.IData.Max => Math.Min(_multiline.Max, _singleline.Max);

        Situation.IData Situation.IData.Add(Situation.IData right)
            => Create(_frame, _multiline.Add(right), _singleline.Add(right));

        Situation.IData Situation.IData.Add(int right)
        {
            NotImplementedMethod(right);
            return null;
        }

        bool? Situation.IData.PreferMultiline => PreferMultiline;

        bool PreferMultiline
        {
            get
            {
                if(_multiline.PreferMultiline == true)
                    return true;
                var maxLineLength = _frame.Formatter.MaxLineLength;
                return maxLineLength != null && _singleline.Max > maxLineLength.Value;
            }
        }

        Rulers Situation.IData.Rulers
            =>
                PreferMultiline
                    ? _multiline.Rulers.Concat(_frame, true)
                    : _singleline.Rulers.Concat(_frame, false);

        Situation.IData Situation.IData.Combine(Frame frame, Situation.IData singleline)
            => singleline.ReverseCombine(frame, this);

        Situation.IData Situation.IData.ReverseAdd(Lengths left)
            => Create(_frame, _multiline.ReverseAdd(left), _singleline.ReverseAdd(left));

        Situation.IData Situation.IData.ReverseCombine(Frame frame, Lengths multiline)
        {
            NotImplementedMethod(frame, multiline);
            return null;
        }

        Situation.IData Situation.IData.ReverseCombine(Frame frame, Variants multiline)
            => Create(frame, multiline, this);
    }
}