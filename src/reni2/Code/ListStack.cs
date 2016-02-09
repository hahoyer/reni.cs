using System;
using System.Collections.Generic;
using System.Linq;
using hw.DebugFormatter;
using hw.Helper;
using Reni.Basics;

namespace Reni.Code
{
    sealed class ListStack : StackData
    {
        [EnableDump]
        readonly NonListStackData[] _data;

        public ListStack(NonListStackData top, NonListStackData formerStackData)
            : base(top.OutStream)
        {
            _data = new[] {top, formerStackData};
        }

        ListStack(NonListStackData[] data, IOutStream outStream)
            : base(outStream)
        {
            _data = data;
        }

        ListStack(NonListStackData[] data, NonListStackData formerStackData)
            : base(formerStackData.OutStream)
        {
            _data = new NonListStackData[data.Length + 1];
            data.CopyTo(_data, 0);
            _data[data.Length] = formerStackData;
        }

        ListStack(NonListStackData[] data, ListStack formerStack)
            : base(formerStack.OutStream)
        {
            _data = new NonListStackData[data.Length + formerStack._data.Length];
            data.CopyTo(_data, 0);
            formerStack._data.CopyTo(_data, data.Length);
        }

        public ListStack(NonListStackData data, ListStack formerStack)
            : base(formerStack.OutStream)
        {
            _data = new NonListStackData[formerStack._data.Length + 1];
            _data[0] = data;
            formerStack._data.CopyTo(_data, 1);
        }

        protected override StackData GetTop(Size size)
        {
            if(_data.Length > 0 && _data[0].Size >= size)
                return _data[0].DoGetTop(size);
            var i = Index(size);
            if(i == null)
                throw new GetTopException(size);

            return Head(i.Value);
        }

        sealed class GetTopException : Exception
        {
            public GetTopException(Size size)
                : base("GetTop failed for size = " + size.Dump()) {}
        }

        int? Index(Size size)
        {
            var sizeSoFar = Size.Zero;
            var i = 0;
            for(; i < _data.Length && sizeSoFar < size; i++)
                sizeSoFar += _data[i].Size;
            if(sizeSoFar > size)
                return null;
            return i;
        }

        protected override StackData Pull(Size size) => Pull(size, false);

        StackData Pull(Size size, bool forced)
        {
            var sizeRemaining = size;
            var start = 0;
            while(start < _data.Length && sizeRemaining.IsPositive)
            {
                sizeRemaining -= _data[start].Size;
                start++;
            }

            var result = Tail(start);
            if(sizeRemaining.IsZero)
                return result;

            var headSize = _data[start - 1].Size + sizeRemaining;

            var headStack = forced
                ? new UnknownStackData(headSize, OutStream)
                : _data[start - 1].DoPull(headSize);

            return result.Push(headStack);
        }

        internal override StackData ForcedPull(Size size) => Pull(size, true);

        StackData Head(int value) => SubString(0, value);
        StackData Tail(int value) => SubString(value, _data.Length - value);

        StackData SubString(int start, int length)
        {
            switch(length)
            {
                case 0:
                    return new EmptyStackData(OutStream);
                case 1:
                    return _data[start];
                default:
                {
                    var newData = new NonListStackData[length];
                    for(var i = 0; i < length; i++)
                        newData[i] = _data[start + i];
                    return new ListStack(newData, OutStream);
                }
            }
        }

        internal override StackData PushOnto(NonListStackData formerStack)
            => new ListStack(_data, formerStack);

        internal override StackData PushOnto(ListStack formerStack)
            => new ListStack(_data, formerStack);

        internal override BitsConst GetBitsConst() => _data.Aggregate(BitsConst.None(), Paste);

        static BitsConst Paste(BitsConst bitsConst, NonListStackData data)
            => bitsConst.Concat(data.GetBitsConst());

        internal override StackData Push(StackData formerStack) => formerStack.PushOnto(this);

        internal override Size Size
        {
            get { return _data.Aggregate(Size.Zero, (current, data) => current + data.Size); }
        }

        public override string DumpData() => _data.Select(DumpItem).Stringify("\n");

        internal override IEnumerable<DataStack.DataMemento> GetItemMementos()
            => _data
            .Select(GetItemMemento)
            .OrderByDescending(item=>item.Offset);

        DataStack.DataMemento GetItemMemento(NonListStackData item, int index)
        {
            return new DataStack.DataMemento(item.Dump())
            {
                Size = item.Size.ToInt(),
                Offset = _data.Skip(index).Sum(item1 => item1.Size.ToInt())
            };
        }

        string DumpItem(NonListStackData item, int index)
        {
            var offset = _data.Skip(index).Sum(item1 => item1.Size.ToInt());
            return "[" + index + "]-" + offset + ": " + item.Dump();
        }
    }

    abstract class NonListStackData : StackData
    {
        protected NonListStackData(IOutStream outStream)
            : base(outStream) {}

        internal override StackData Push(StackData stackData) => stackData.PushOnto(this);

        internal override StackData PushOnto(NonListStackData formerStack)
            => new ListStack(this, formerStack);

        internal override StackData PushOnto(ListStack formerStack)
            => new ListStack(this, formerStack);
    }
}