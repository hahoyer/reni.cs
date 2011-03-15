using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace Reni.Runtime
{
    [UsedImplicitly]
    public sealed class DataContainer : ReniObject
    {
        private byte[] _data;
        public DataContainer(params byte[] data) { _data = data; }

        public DataContainer(int count, byte[] data)
        {
            _data = new byte[count];
            for(var i = 0; i < count; i++)
                _data[i] = data[i];
        }

        private unsafe DataContainer(int count, byte* data)
        {
            _data = new byte[count];
            for(var i = 0; i < count; i++)
                _data[i] = data[i];
        }

        [UsedImplicitly]
        public DataContainer DumpPrint()
        {
            Data.DumpPrint(new BigInteger(_data).ToString());
            return new DataContainer();
        }

        [UsedImplicitly]
        public static DataContainer DumpPrint(string text)
        {
            Data.DumpPrint(text);
            return new DataContainer();
        }

        [UsedImplicitly]
        public DataContainer DataPart(int offset, int bytes)
        {
            if(bytes == _data.Length)
            {
                Tracer.Assert(offset == 0);
                return this;
            }
            var result = new byte[bytes];
            for(var i = 0; i < bytes; i++)
                result[i] = _data[offset + i];
            return new DataContainer(result);
        }

        [UsedImplicitly]
        public DataContainer DataPartFromBack(int offset, int bytes) { return DataPart(_data.Length + offset, bytes); }

        [UsedImplicitly]
        public DataContainer BitCast(int bytes, int bits)
        {
            Tracer.Assert(bytes == _data.Length);
            var newData = new byte[bytes];
            _data.CopyTo(newData, 0);
            Data.BitCast(newData, bits);
            return new DataContainer(newData);
        }

        [UsedImplicitly]
        public void Expand(DataContainer dataContainer)
        {
            if(dataContainer._data.Length == 0)
                return;
            var oldData = _data;
            _data = new byte[oldData.Length + dataContainer._data.Length];
            dataContainer._data.CopyTo(_data, 0);
            oldData.CopyTo(_data, dataContainer._data.Length);
        }

        [UsedImplicitly]
        public DataContainer Plus(int bytes, int leftBytes)
        {
            var d1 = new BigInteger(DataPart(0, leftBytes)._data);
            var d2 = new BigInteger(DataPart(leftBytes, _data.Length - leftBytes)._data);
            return new DataContainer((d1 + d2).ToByteArray());
        }

        [UsedImplicitly]
        public void Drop(int bytes) { Tracer.Assert(_data.Length == bytes); }

        [UsedImplicitly]
        public void DropAll() { Drop(_data.Length); }

        [UsedImplicitly]
        public DataContainer Minus(int bytes)
        {
            Tracer.Assert(bytes == _data.Length);
            return new DataContainer((-new BigInteger(_data)).ToByteArray());
        }

        [UsedImplicitly]
        public unsafe DataContainer DataRef(int offset)
        {
            fixed(byte* p = &_data[offset])
            {
                var ip = (int) p;
                var ipp = (byte*) &ip;
                return new DataContainer(sizeof(int), ipp);
            }
        }

        [UsedImplicitly]
        public unsafe DataContainer Dereference(int bytes)
        {
            fixed(byte* p = _data)
            {
                var ip = (int*) p;
                var ipp = (byte*) *ip;
                return new DataContainer(bytes, ipp);
            }
        }

        [UsedImplicitly]
        public DataContainer Call(Func<DataContainer, DataContainer> func) { return func(this); }
    }
}