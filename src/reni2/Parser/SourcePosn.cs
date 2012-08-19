using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    /// <summary>
    ///     Source and position for compilation process
    /// </summary>
    [Serializable]
    internal sealed class SourcePosn : ReniObject
    {
        private readonly Source _source;
        private int _position;

        /// <summary>
        ///     ctor from source and position
        /// </summary>
        /// <param name = "source"></param>
        /// <param name = "position"></param>
        public SourcePosn(Source source, int position)
        {
            _position = position;
            _source = source;
        }

        public Source Source { get { return _source; } }
        public int Position { get { return _position; } }

        /// <summary>
        ///     The current character
        /// </summary>
        public char Current { get { return _source[_position]; } }

        /// <summary>
        ///     Natuaral indexer
        /// </summary>
        public char this[int index] { get { return _source[_position + index]; } }

        /// <summary>
        ///     Advance position
        /// </summary>
        /// <param name = "i">number characters to move</param>
        public void Incr(int i) { _position += i; }

        /// <summary>
        ///     Checks if at or beyond end of source
        /// </summary>
        /// <returns></returns>
        public bool IsEnd() { return _source.IsEnd(_position); }

        /// <summary>
        ///     Obtains a piece
        /// </summary>
        /// <param name = "start">start position</param>
        /// <param name = "length">number of characters</param>
        /// <returns></returns>
        public string SubString(int start, int length) { return _source.SubString(_position + start, length); }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name = "flagText">the flag text</param>
        /// <returns>the "FileName(LineNr,ColNr): tag: " string</returns>
        public string FilePosn(string flagText) { return _source.FilePosn(_position, flagText); }

        /// <summary>
        ///     Default dump behaviour
        /// </summary>
        /// <returns>The file position of sourec file</returns>
        protected override string Dump(bool isRecursion) { return "\n"+FilePosn("see there"); }
    }
}