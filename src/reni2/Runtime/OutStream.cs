using hw.DebugFormatter;

namespace Reni.Runtime
{
    /// <summary>
    ///     Handle printouts
    /// </summary>
    public sealed class OutStream : DumpableObject, IOutStream
    {
        private string _data = "";
        private string _log = "";

        internal string Data => _data;
        internal string Log => _log;

        void IOutStream.AddData(string x)
        {
            _data += x;
            Tracer.Log("-data----------------\n" + _data + "|<--\n---------------------");
            Tracer.Assert(_data.Length < 1000);
        }

        void IOutStream.AddLog(string x)
        {
            _log += x;
            Tracer.Log("-log----------------\n" + _log + "\n---------------------");
        }
    }
}