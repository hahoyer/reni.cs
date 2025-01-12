namespace Reni.Runtime
{
    /// <summary>
    ///     Handle printouts
    /// </summary>
    public sealed class OutStream : DumpableObject, IOutStream
    {
        [PublicAPI]
        internal string Data = "";
        [PublicAPI]
        internal string Log = "";

        void IOutStream.AddData(string x)
        {
            Data += x;
            ("-data----------------\n" + Data + "|<--\n---------------------").Log();
            (Data.Length < 1000).Assert();
        }

        void IOutStream.AddLog(string x)
        {
            Log += x;
            ("-log----------------\n" + Log + "\n---------------------").Log();
        }
    }
}