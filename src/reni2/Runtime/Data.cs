//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using JetBrains.Annotations;
using Reni.Basics;

namespace Reni.Runtime
{
    [DebuggerDisplay("{Dump}")]
    public sealed class Data
    {
        readonly byte[] _data;
        int _length;

        public static Data Create(int bytes) { return new Data(new byte[bytes]); }
        public static Data Create(byte[] bytes) { return new Data(bytes) {StartIndex = 0}; }

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

        public void Push(Data data) { Push(data.GetBytes(data._length)); }

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

        public byte[] GetBytes() { return GetBytes(_length); }

        int StartIndex
        {
            get { return _data.Length - _length; }
            set
            {
                _length = _data.Length - value;
                EnsureLength();
            }
        }
        void EnsureLength() { Tracer.Assert(StartIndex >= 0); }

        public Data Address(int offset) { return Create(_data.Address(StartIndex + offset)); }
        public void RefPlus(int offset) { _data.DoRefPlus(StartIndex, offset); }
        public Data Dereference(int bytes) { return Create(_data.Dereference(StartIndex, bytes)); }
        public Data Get(int bytes, int offset) { return Create(_data.Get(StartIndex + offset, bytes)); }
        public Data GetFromBack(int bytes, int offset) { return Create(_data.Get(_data.Length + offset, bytes)); }
        public void PrintNumber() { GetBytes().PrintNumber(); }
        public void PrintText(int itemBytes)
        {
            Tracer.Assert(itemBytes == 1);
            GetBytes(_length).PrintText();
        }
        public static void PrintText(string data) { data.PrintText(); }

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

        string Dump
        {
            get
            {
                var result = _data.Length.ToString() + ":";
                for(var i = 0; i < _data.Length; i++)
                {
                    if(StartIndex == i)
                        result += " [" + _length + ":";

                    result += " ";
                    result += _data[i].ToString();
                }
                return result + " ]";
            }
        }

        [UsedImplicitly]
        public void Equal(int leftBytes, int rightBytes) { Compare(leftBytes, rightBytes, DataHandler.IsEqual); }
        [UsedImplicitly]
        public void LessGreater(int leftBytes, int rightBytes) { Compare(leftBytes, rightBytes, DataHandler.IsNotEqual); }
        [UsedImplicitly]
        public void Less(int leftBytes, int rightBytes) { Compare(leftBytes, rightBytes, DataHandler.IsLess); }
        [UsedImplicitly]
        public void Greater(int leftBytes, int rightBytes) { Compare(leftBytes, rightBytes, DataHandler.IsGreater); }
        [UsedImplicitly]
        public void LessEqual(int leftBytes, int rightBytes) { Compare(leftBytes, rightBytes, DataHandler.IsLessEqual); }
        [UsedImplicitly]
        public void GreaterEqual(int leftBytes, int rightBytes) { Compare(leftBytes, rightBytes, DataHandler.IsGreaterEqual); }

        void Compare(int leftBytes, int rightBytes, Func<byte[], byte[], bool> operation)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            Push((byte)(operation(left._data, right._data) ? -1 : 0));
        }

        [UsedImplicitly]
        public void MinusPrefix(int bytes)
        {
            var data = Pull(bytes);
            DataHandler.MinusPrefix(data._data);
            Push(data);
        }

        [UsedImplicitly]
        public void Plus(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            Push(DataHandler.Plus(sizeBytes, left._data, right._data));
        }

        [UsedImplicitly]
        public void Plus(int leftBytes, int rightBytes) { Plus(1, leftBytes, rightBytes); }
        [UsedImplicitly]
        public void Minus(int leftBytes, int rightBytes) { Minus(1, leftBytes, rightBytes); }

        [UsedImplicitly]
        public void Minus(int sizeBytes, int leftBytes, int rightBytes)
        {
            var right = Pull(rightBytes);
            var left = Pull(leftBytes);
            right.MinusPrefix(rightBytes);
            Push(DataHandler.Plus(sizeBytes, left._data, right._data));
        }
    }

}

