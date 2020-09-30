using System.Text;
using hw.DebugFormatter;

namespace Reni
{
    public sealed class StringStream : DumpableObject, IOutStream
    {
        readonly StringBuilder _data = new StringBuilder();
        readonly StringBuilder _log = new StringBuilder();
        internal string Data => _data.ToString();
        internal string Log => _log.ToString();
        void IOutStream.AddData(string text) => _data.Append(text);
        void IOutStream.AddLog(string text) => _log.Append(text);
    }
}