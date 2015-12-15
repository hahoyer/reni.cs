using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using hw.DebugFormatter;
using hw.Helper;
using JetBrains.Annotations;
using Reni.Basics;

namespace Reni.Runtime
{
    [DebuggerDisplay("{Dump}")]
    public sealed class Data
    {
        readonly byte[] _data;
        int _length;

        public static IOutStream OutStream { internal get; set; }

        public static Data Create(int bytes) => new Data(new byte[bytes]);

        public static Data Create(byte[] bytes) => new Data(bytes)
        {
            StartIndex = 0
        };

        Data(byte[] result)
        {
            _data = result;
            _length = 0;
        }

        public void Push(params byte[] bytes)
        {
            StartIndex -= bytes.Length;
            bytes.CopyTo(_data, StartIndex);
        }

        public void SizedPush(int byteCount, params byte[] bytes)
        {
            StartIndex -= byteCount;
            var i = 0;
            for(; i < byteCount && i < bytes.Length; i++)
                _data[StartIndex + i] = bytes[i];
            for(; i < byteCount; i++)
                _data[StartIndex + i] = 0;
        }

        public void Push(Data data) => Push(data.GetBytes(data._length));

        public Data Pull(int bytes)
        {
            var result = new Data(GetBytes(bytes));
            result.StartIndex -= bytes;
            StartIndex += bytes;
            return result;
        }

        public void Drop(int bytes) { StartIndex += bytes; }

        public void Drop(int bytesBefore, int bytesAfter)
        {
            var top = Pull(bytesAfter);
            StartIndex += bytesBefore - bytesAfter;
            Push(top);
        }

        public byte[] GetBytes(int count)
        {
            var result = new byte[count];
            for(var i = 0; i < count; i++)
                result[i] = _data[StartIndex + i];
            return result;
        }

        public byte[] GetBytes() => GetBytes(_length);

        int StartIndex
        {
            get { return _data.Length - _length; }
            set
            {
                _length = _data.Length - value;
                EnsureLength();
            }
        }

        void EnsureLength() => Tracer.Assert(StartIndex >= 0);

        public Data Pointer(int offset) => Create(_data.Pointer(StartIndex + offset));
        public void PointerPlus(int offset) => _data.DoRefPlus(StartIndex, offset);
        public Data DePointer(int bytes) => Create(_data.Dereference(StartIndex, bytes));
        public Data Get(int bytes, int offset) => Create(_data.Get(StartIndex + offset, bytes));

        public Data GetFromBack(int bytes, int offset)
            => Create(_data.Get(_data.Length + offset, bytes));

        public void PrintNumber() => GetBytes().PrintNumber();

        public void PrintText(int itemBytes)
        {
            Tracer.Assert(itemBytes == 1);
            GetBytes(_length).PrintText();
        }

        public static void PrintText(string data) => data.PrintText();

        public void Assign(int bytes)
        {
            var right = Pull(DataHandler.RefBytes);
            var left = Pull(DataHandler.RefBytes);
            left._data.AssignFromPointers(right._data, bytes);
        }

        public void ArrayGetter(int elementBytes, int indexBytes)
        {
            var offset = Pull(indexBytes).GetBytes().Times(elementBytes, DataHandler.RefBytes);
            var baseAddress = Pull(DataHandler.RefBytes);
            Push(baseAddress.GetBytes().Plus(offset, DataHandler.RefBytes));
        }

        public Data BitCast(int bits)
        {
            var x = BitsConst.Convert(_data);
            var y = x.Resize(Size.Create(bits));
            return Create(y.ToByteArray());
        }

        [UsedImplicitly]
        string Dump => Pointer(0).AddressDump + ": " + DataDump;

        string DataDump => DataDumpPart();

        string DataDumpPart()
        {
            var result = "";
            result += DumpRange(0, StartIndex);
            result += " ";
            result += DumpLengthAndRange(StartIndex, _length);
            return result;
        }

        string DumpLengthAndRange(int startIndex, int bytes)
            => "[" + bytes + ": " + DumpRange(startIndex, bytes) + "]";

        string DumpRange(int startIndex, int bytes)
        {
            if(bytes <= 0)
                return "";

            return 
                bytes
                .Select(i => startIndex + i)
                .Where(i => i + DataHandler.RefBytes < _length)
                .Select(i => _biasCache.Dump(Get(DataHandler.RefBytes, i)))
                    .Stringify(" ");
        }

        IEnumerable<byte> Range(int startIndex, int bytes) => _data.Skip(startIndex).Take(bytes);

        static readonly BiasCache _biasCache = new BiasCache(100);
        [UsedImplicitly]
        string AddressDump => _biasCache.AddressDump(this) + "=" + DataDump;

        public IView GetCurrentView(int bytes) => new View(this, StartIndex, bytes);


        public interface IView {}

        public sealed class View : DumpableObject, IView
        {
            readonly Data _parent;
            readonly int _startIndex;
            readonly int _bytes;

            public View(Data parent, int startIndex, int bytes)
            {
                _parent = parent;
                _startIndex = startIndex;
                _bytes = bytes;
            }

            protected override string GetNodeDump()
                => Create(_parent._data.Pointer(_startIndex)).AddressDump
                    + ": "
                    + _parent.DumpLengthAndRange(_startIndex, _bytes);
        }

        [UsedImplicitly]
        public void Equal(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsEqual);

        [UsedImplicitly]
        public void LessGreater(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsNotEqual);

        [UsedImplicitly]
        public void Less(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsLess);

        [UsedImplicitly]
        public void Greater(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsGreater);

        [UsedImplicitly]
        public void LessEqual(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsLessEqual);

        [UsedImplicitly]
        public void GreaterEqual(int sizeBytes, int leftBytes, int rightBytes)
            => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsGreaterEqual);

        void Compare
            (int sizeBytes, int leftBytes, int rightBytes, Func<byte[], byte[], bool> operation)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            var value = (byte) (operation(left._data, right._data) ? -1 : 0);
            for(var i = 0; i < sizeBytes; i++)
                Push(value);
        }

        [UsedImplicitly]
        public void MinusPrefix(int bytes)
        {
            var data = Pull(bytes);
            data._data.MinusPrefix();
            Push(data);
        }

        [UsedImplicitly]
        public void Plus(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            Push(left._data.Plus(right._data, sizeBytes));
        }

        [UsedImplicitly]
        public void Minus(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            right.MinusPrefix(rightBytes);
            Push(left._data.Plus(right._data, sizeBytes));
        }

        [UsedImplicitly]
        public void Star(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            Push(left._data.Times(right._data, sizeBytes));
        }
    }
}