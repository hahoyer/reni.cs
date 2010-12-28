using System;
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

        internal override StackData GetTop(Size size)
        {
            if(_data.Length > 0 && _data[0].Size == size)
                return _data[0];
            return base.GetTop(size);
        }

        internal override StackData Pull(Size size)
        {
            var sizeSoFar = Size.Zero;
            var i = 0;
            for(; i < _data.Length && sizeSoFar < size; i++)
                sizeSoFar += _data[i].Size;
            
            return sizeSoFar == size ? Tail(i) : base.Pull(size);
        }

        private StackData Tail(int i)
        {
            var newLength = _data.Length - i;
            if(newLength == 0)
                return new EmptyStackData();
            if(newLength == 1)
                return _data[i];
            var newData = new NonListStackData[newLength];
            for(var j = 0; j < newLength; j++)
                newData[j] = _data[i + j];
            return new ListStack(newData);
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


