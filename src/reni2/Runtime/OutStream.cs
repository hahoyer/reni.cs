using HWClassLibrary.Debug;

namespace Reni.Runtime
{
    /// <summary>
    /// Handle printouts
    /// </summary>
    public class OutStream: ReniObject
    {
        string _data = "";

        /// <summary>
        /// Content so far
        /// </summary>
        public string Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Adds some text
        /// </summary>
        /// <param name="x"></param>
        public void Add(string x)
        {
            _data += x;
            Exec();
            Tracer.Assert(_data.Length < 1000);
        }

        /// <summary>
        /// put it out
        /// </summary>
        public void Exec()
        {
            Tracer.Line("---------------------\n" + _data + "\n---------------------");
        }
    }
}
