using System.Text;
using hw.DebugFormatter;

namespace Reni
{
    public sealed class StringStream : DumpableObject, IOutStream
    {
        readonly StringBuilder DataCache = new();
        readonly StringBuilder LogCache = new();
        void IOutStream.AddData(string text) => DataCache.Append(text);
        void IOutStream.AddLog(string text) => LogCache.Append(text);
        internal string Data => DataCache.ToString();
        internal string Log => LogCache.ToString();
    }
}