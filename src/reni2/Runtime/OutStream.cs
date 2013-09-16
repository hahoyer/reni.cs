using System;
using System.Collections.Generic;
using System.Linq;
using hw.Debug;

namespace Reni.Runtime
{
    /// <summary>
    ///     Handle printouts
    /// </summary>
    public sealed class OutStream : DumpableObject, IOutStream
    {
        private string _data = "";
        private string _log = "";

        internal string Data { get { return _data; } }
        internal string Log { get { return _log; } }

        void IOutStream.AddData(string x)
        {
            _data += x;
            Tracer.Line("-data----------------\n" + _data + "|<--\n---------------------");
            Tracer.Assert(_data.Length < 1000);
        }

        void IOutStream.AddLog(string x)
        {
            _log += x;
            Tracer.Line("-log----------------\n" + _log + "\n---------------------");
        }
    }
}