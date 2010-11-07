using System;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace Reni.Runtime
{
    [UsedImplicitly]
    public sealed class DataContainer
    {
        private byte[] _data;
        public DataContainer(params byte[] data)
        {
            _data = data;
        }

        [UsedImplicitly]
        public DataContainer DumpPrint(int bits)
        {
            Data.DumpPrint(_data);
            return new DataContainer();
        }
        [UsedImplicitly]
        public DataContainer DataPart(int bytes)
        {
            Tracer.Assert(bytes == _data.Length);
            return this;
        }
        [UsedImplicitly]
        public DataContainer BitCast(int bytes, int bits)
        {
            Tracer.Assert(bytes == _data.Length);
            var newData = new byte[bytes];
            _data.CopyTo(newData,0);
            Data.BitCast(newData,bits);
            return new DataContainer(newData);
        }
        [UsedImplicitly]
        public void Expand(DataContainer dataContainer)
        {
            var oldData = _data;
            _data = new byte[oldData.Length + dataContainer._data.Length];
            oldData.CopyTo(_data,0);
            dataContainer._data.CopyTo(_data, oldData.Length);
        }
        [UsedImplicitly]
        public unsafe DataContainer Plus(int bytes, int leftBytes)
        {
            var newData = new byte[bytes];
            Data.Plus(newData, leftBytes, _data);
            return new DataContainer(newData);
        }
        [UsedImplicitly]
        public void Drop() { Tracer.Assert(_data.Length == 0); }
    }
}