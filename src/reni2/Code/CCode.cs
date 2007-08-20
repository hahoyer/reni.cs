using System;

namespace Reni.Code
{
    /// <summary>
    /// C-Code generator visitor
    /// </summary>
    sealed class CCode : Visitor<string>
    {
        private readonly int _baseAddr;

        /// <summary>
        /// Gets the base addr.
        /// </summary>
        /// <value>The base addr.</value>
        /// [created 20.07.2006 23:18]
        public int BaseAddr { get { return _baseAddr; } }

        private CCode(int baseAddr)
        {
            _baseAddr = baseAddr;
        }

        /// <summary>
        /// Roots the specified base addr.
        /// </summary>
        /// <param name="baseAddr">The base addr.</param>
        /// <returns></returns>
        /// [created 20.07.2006 23:18]
        public static CCode Root(int baseAddr)
        {
            return new CCode(baseAddr);
        }

        private CCode Shift(int byteCount)
        {
            return new CCode(BaseAddr - byteCount);
        }

        /// <summary>
        /// Toes the C code sequence.
        /// </summary>
        /// <param name="byteCount">The byte count.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// [created 18.07.2006 23:07]
        string DataSet(int byteCount, string value)
        {
            if (value == "")
                return "";
            string result = value + ";\n";
            if (byteCount == 0)
                return result;
            return DataGet(byteCount) + " = " + result;
        }

        /// <summary>
        /// Toes the C code sequence.
        /// </summary>
        /// <param name="byteCount">The byte count.</param>
        /// <returns></returns>
        /// [created 18.07.2006 23:07]
        string DataGet(int byteCount)
        {
            return DataGet(BaseAddr, byteCount);
        }

        /// <summary>
        /// Toes the C code sequence.
        /// </summary>
        /// <param name="baseAddr">The base addr.</param>
        /// <param name="byteCount">The byte count.</param>
        /// <returns></returns>
        /// [created 18.07.2006 23:07]
        string DataGet(int baseAddr, int byteCount)
        {
            if (byteCount == 0)
                return "";
            return Data + "(" + (baseAddr - byteCount) + ", " + byteCount + ")";
        }

        /// <summary>
        /// Gets to C code data.
        /// </summary>
        /// <value>To C code data.</value>
        /// [created 19.07.2006 23:38]
        static string Data { get { return "_data"; } }

        /// <summary>
        /// Toes the C code sequence.
        /// </summary>
        /// <param name="baseAddr">The base addr.</param>
        /// <returns></returns>
        /// [created 20.07.2006 23:15]
        public string ToCCodeSequence(int baseAddr)
        {
            NotImplementedMethod(baseAddr);
            throw new NotImplementedException();
        }
    }
}
