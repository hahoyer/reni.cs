using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using hw.Debug;
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

        public void Push(Data data) => Push(data.GetBytes(data._length));

        public Data Pull(int bytes)
        {
            var result = new Data(GetBytes(bytes));
            result.StartIndex -= bytes;
            StartIndex += bytes;
            return result;
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
        public Data GetFromBack(int bytes, int offset) => Create(_data.Get(_data.Length + offset, bytes));
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

        public Data BitCast(int bits)
        {
            var x = BitsConst.Convert(_data);
            var y = x.Resize(Size.Create(bits));
            return Create(y.ToByteArray());
        }

        string Dump { get { return Pointer(0).AddressDump + ": " + DataDump; } }

        string DataDump
        {
            get
            {
                var result = "";
                for(var i = 0; i < _data.Length; i++)
                {
                    if(StartIndex == i)
                        result += "[" + _length + ":";

                    result += _data[i].ToString();
                    if(i < _data.Length - 1)
                        result += " ";
                }
                if(StartIndex == _data.Length)
                    result += " [" + _length + ":";
                return result + "]";
            }
        }

        static readonly BiasCache _biasCache = new BiasCache(100);
        [UsedImplicitly]
        string AddressDump { get { return _biasCache.AddressDump(this) + "=" + DataDump; } }

        [UsedImplicitly]
        public void Equal(int sizeBytes, int leftBytes, int rightBytes) => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsEqual);
        [UsedImplicitly]
        public void LessGreater(int sizeBytes, int leftBytes, int rightBytes) => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsNotEqual);
        [UsedImplicitly]
        public void Less(int sizeBytes, int leftBytes, int rightBytes) => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsLess);
        [UsedImplicitly]
        public void Greater(int sizeBytes, int leftBytes, int rightBytes) => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsGreater);
        [UsedImplicitly]
        public void LessEqual(int sizeBytes, int leftBytes, int rightBytes) => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsLessEqual);
        [UsedImplicitly]
        public void GreaterEqual(int sizeBytes, int leftBytes, int rightBytes) => Compare(sizeBytes, leftBytes, rightBytes, DataHandler.IsGreaterEqual);

        void Compare(int sizeBytes, int leftBytes, int rightBytes, Func<byte[], byte[], bool> operation)
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