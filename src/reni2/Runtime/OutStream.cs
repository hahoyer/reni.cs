using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Runtime
{
    /// <summary>
    ///     Handle printouts
    /// </summary>
    public sealed class OutStream : ReniObject, IOutStream
    {
        private string _data = "";

        internal string Data { get { return _data; } }

        void IOutStream.Add(string x)
        {
            _data += x;
            Tracer.Line("---------------------\n" + _data + "\n---------------------");
            Tracer.Assert(_data.Length < 1000);
        }
    }
}