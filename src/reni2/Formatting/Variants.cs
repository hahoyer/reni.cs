using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Formatting
{
    sealed class Variants : DumpableObject, Situation.IData
    {
        [EnableDump]
        readonly Lengths _multiline;
        [EnableDump]
        readonly Lengths _singleline;
        [EnableDump]
        readonly Frame _frame;

        public Variants(Frame frame, Lengths multiline, Lengths singleline)
        {
            _frame = frame;
            _multiline = multiline;
            _singleline = singleline;
        }

        Situation.IData Situation.IData.Add(Situation.IData right)
        {
            NotImplementedMethod(right);
            return null;

        }

        Situation.IData Situation.IData.Add(int right)
        {
            NotImplementedMethod(right);
            return null;
        }

        Situation.IData Situation.IData.Combine(Frame frame, Situation.IData singleline)
        {
            NotImplementedMethod(frame, singleline);
            return null;
        }

        Situation.IData Situation.IData.ReverseAdd(Lengths left)
        {
            NotImplementedMethod(left);
            return null;
        }

        Situation.IData Situation.IData.ReverseCombine(Frame frame, Lengths multiline)
        {
            NotImplementedMethod(frame,multiline);
            return null;
        }
    }
}