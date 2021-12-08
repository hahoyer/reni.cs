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
        readonly NonListStackData[] Data;

        public ListStack(NonListStackData top, NonListStackData formerStackData)
            : base(top.OutStream)
        {
            Data = new[] {top, formerStackData};
        }

        ListStack(NonListStackData[] data, IOutStream outStream)
            : base(outStream)
        {
            Data = data;
        }

        ListStack(NonListStackData[] data, NonListStackData formerStackData)
            : base(formerStackData.OutStream)
        {
            Data = new NonListStackData[data.Length + 1];
            data.CopyTo(Data, 0);
            Data[data.Length] = formerStackData;
        }

        ListStack(NonListStackData[] data, ListStack formerStack)
            : base(formerStack.OutStream)
        {
            Data = new NonListStackData[data.Length + formerStack.Data.Length];
            data.CopyTo(Data, 0);
            formerStack.Data.CopyTo(Data, data.Length);
        }

        public ListStack(NonListStackData data, ListStack formerStack)
            : base(formerStack.OutStream)
        {
            Data = new NonListStackData[formerStack.Data.Length + 1];
            Data[0] = data;
            formerStack.Data.CopyTo(Data, 1);
        }

        protected override StackData GetTop(Size size)
        {
            var i = Index(size);
            var result = Head(i);
            if(result.Size == size)
                return result;

            return Data[i].DoGetTop(size - result.Size).Push(result);
        }

        int Index(Size size)
        {
            var sizeSoFar = Size.Zero;
            var i = 0;
            for(; i < Data.Length && sizeSoFar <= size; i++)
                sizeSoFar += Data[i].Size;
            return i-1;
        }

        protected override StackData Pull(Size size) => Pull(size, false);

        StackData Pull(Size size, bool forced)
        {
            var sizeRemaining = size;
            var start = 0;
            while(start < Data.Length && sizeRemaining.IsPositive)
            {
                sizeRemaining -= Data[start].Size;
                start++;
            }

            var result = Tail(start);
            if(sizeRemaining.IsZero)
                return result;

            var headSize = Data[start - 1].Size + sizeRemaining;

            var headStack = forced
                ? new UnknownStackData(headSize, OutStream)
                : Data[start - 1].DoPull(headSize);

            return result.Push(headStack);
        }

        internal override StackData ForcedPull(Size size) => Pull(size, true);

        StackData Head(int value) => SubString(0, value);
        StackData Tail(int value) => SubString(value, Data.Length - value);

        StackData SubString(int start, int length)
        {
            switch(length)
            {
                case 0:
                    return new EmptyStackData(OutStream);
                case 1:
                    return Data[start];
                default:
                {
                    var newData = new NonListStackData[length];
                    for(var i = 0; i < length; i++)
                        newData[i] = Data[start + i];
                    return new ListStack(newData, OutStream);
                }
            }
        }

        internal override StackData PushOnto(NonListStackData formerStack)
            => new ListStack(Data, formerStack);

        internal override StackData PushOnto(ListStack formerStack)
            => new ListStack(Data, formerStack);

        internal override BitsConst GetBitsConst() => Data.Aggregate(BitsConst.None(), Paste);

        static BitsConst Paste(BitsConst bitsConst, NonListStackData data)
            => bitsConst.Concat(data.GetBitsConst());

        internal override StackData Push(StackData formerStack) => formerStack.PushOnto(this);

        internal override Size Size
        {
            get { return Data.Aggregate(Size.Zero, (current, data) => current + data.Size); }
        }

        public override string DumpData() => Data.Select(DumpItem).Stringify("\n");

        internal override IEnumerable<DataStack.DataMemento> GetItemMementos()
            => Data
            .Select(GetItemMemento)
            .OrderByDescending(item=>item.Offset);

        DataStack.DataMemento GetItemMemento(NonListStackData item, int index)
        {
            return new DataStack.DataMemento(item.Dump())
            {
                Size = item.Size.ToInt(),
                Offset = Data.Skip(index).Sum(item1 => item1.Size.ToInt())
            };
        }

        string DumpItem(NonListStackData item, int index)
        {
            var offset = Data.Skip(index).Sum(item1 => item1.Size.ToInt());
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