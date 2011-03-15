﻿using System;
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
        private readonly Source _text;
        private int _posn;

        /// <summary>
        ///     ctor from source and position
        /// </summary>
        /// <param name = "text"></param>
        /// <param name = "posn"></param>
        public SourcePosn(Source text, int posn)
        {
            _posn = posn;
            _text = text;
        }

        /// <summary>
        ///     The current character
        /// </summary>
        public char Current { get { return _text[_posn]; } }

        /// <summary>
        ///     Natuaral indexer
        /// </summary>
        public char this[int index] { get { return _text[_posn + index]; } }

        /// <summary>
        ///     Advance position
        /// </summary>
        /// <param name = "i">number characters to move</param>
        public void Incr(int i) { _posn += i; }

        /// <summary>
        ///     Checks if at or beyond end of source
        /// </summary>
        /// <returns></returns>
        public bool IsEnd() { return _text.IsEnd(_posn); }

        /// <summary>
        ///     Obtains a piece
        /// </summary>
        /// <param name = "start">start position</param>
        /// <param name = "length">number of characters</param>
        /// <returns></returns>
        public string SubString(int start, int length) { return _text.SubString(_posn + start, length); }

        /// <summary>
        ///     asis
        /// </summary>
        /// <returns></returns>
        public SourcePosn Clone() { return new SourcePosn(_text, _posn); }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name = "flagText">the flag text</param>
        /// <returns>the "FileName(LineNr,ColNr): flagText: " string</returns>
        public string FilePosn(string flagText) { return _text.FilePosn(_posn, flagText); }

        /// <summary>
        ///     Default dump behaviour
        /// </summary>
        /// <returns>The file position of sourec file</returns>
        protected override string Dump(bool isRecursion) { return FilePosn("see there"); }
    }
}