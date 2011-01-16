﻿using System;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Code
{
    internal sealed class ListStack : StackData
    {
        [IsDumpEnabled(true)]
        private readonly NonListStackData[] _data;

        public ListStack(NonListStackData top, NonListStackData formerStackData)
        {
            _data = new NonListStackData[2] { top, formerStackData };
        }

        private ListStack(NonListStackData[] data) { _data = data; }

        private ListStack(NonListStackData[] data, NonListStackData formerStackData)
        {
            _data = new NonListStackData[data.Length + 1];
            data.CopyTo(_data, 0);
            _data[data.Length] = formerStackData;
        }

        private ListStack(NonListStackData[] data, NonListStackData[] formerStackData)
        {
            _data = new NonListStackData[data.Length + formerStackData.Length];
            data.CopyTo(_data, 0);
            formerStackData.CopyTo(_data, data.Length);
        }

        public ListStack(NonListStackData data, NonListStackData[] formerStackData)
        {
            _data = new NonListStackData[formerStackData.Length + 1];
            _data[0] = data;
            formerStackData.CopyTo(_data, 1);
        }

        protected override StackData GetTop(Size size)
        {
            var i = Index(size);
            if (i == null)
                return base.GetTop(size);

            return Head(i.Value);
        }

        private int? Index(Size size)
        {
            var sizeSoFar = Size.Zero;
            var i = 0;
            for(; i < _data.Length && sizeSoFar < size; i++)
                sizeSoFar += _data[i].Size;
            if (sizeSoFar > size)
                return null;
            return i;
        }

        protected override StackData Pull(Size size)
        {
            var i = Index(size);
            if(i == null)
                return base.Pull(size);
            return Tail(i.Value);
        }

        private StackData Head(int value) { return SubString(0, value); }
        private StackData Tail(int value) { return SubString(value, _data.Length - value); }

        private StackData SubString(int start, int length)
        {
            switch (length)
            {
                case 0:
                    return new EmptyStackData();
                case 1:
                    return _data[start];
                default:
                    {
                        var newData = new NonListStackData[length];
                        for (var i = 0; i < length; i++)
                            newData[i] = _data[start + i];
                        return new ListStack(newData);
                    }
            }
        }
        internal override StackData PushOnto(NonListStackData formerStack) { return new ListStack(_data, formerStack); }
        internal override StackData PushOnto(NonListStackData[] formerStack) { return new ListStack(_data, formerStack); }
        internal override StackData Push(StackData formerStack) { return formerStack.PushOnto(_data); }
        internal override Size Size { get { return _data.Aggregate(Size.Zero, (current, data) => current + data.Size); } }
    }

    internal abstract class NonListStackData: StackData
    {
        internal override StackData Push(StackData stackData) { return stackData.PushOnto(this); }
        internal override StackData PushOnto(NonListStackData formerStack) { return new ListStack(this, formerStack); }
        internal override StackData PushOnto(NonListStackData[] formerStack) { return new ListStack(this, formerStack); }
    }
}


