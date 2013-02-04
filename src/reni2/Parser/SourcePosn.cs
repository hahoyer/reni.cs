#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//     
//     Comments, bugs and suggestions to hahoyer at yahoo.de

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HWClassLibrary.Debug;
using JetBrains.Annotations;

namespace Reni.Parser
{
    /// <summary>
    ///     Source and position for compilation process
    /// </summary>
    [DebuggerDisplay("{NodeDump} {DumpBeforeCurrent}[{DumpCurrent}]{DumpAfterCurrent}")]
    sealed class SourcePosn : ReniObject
    {
        readonly Source _source;
        int _position;

        /// <summary>
        ///     ctor from source and position
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
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
        /// <param name="i">number characters to move</param>
        public void Incr(int i = 1) { _position += i; }

        /// <summary>
        ///     Checks if at or beyond end of source
        /// </summary>
        /// <value> </value>
        public bool IsEnd { get { return _source.IsEnd(_position); } }

        /// <summary>
        ///     Obtains a piece
        /// </summary>
        /// <param name="start">start position</param>
        /// <param name="length">number of characters</param>
        /// <returns></returns>
        public string SubString(int start, int length) { return _source.SubString(_position + start, length); }

        /// <summary>
        ///     creates the file(line,col) string to be used with "Edit.GotoNextLocation" command of IDE
        /// </summary>
        /// <param name="flagText">the flag text</param>
        /// <returns>the "FileName(LineNr,ColNr): tag: " string</returns>
        public string FilePosn(string flagText) { return _source.FilePosn(_position, flagText); }

        /// <summary>
        ///     Default dump behaviour
        /// </summary>
        /// <returns>The file position of sourec file</returns>
        protected override string Dump(bool isRecursion) { return "\n" + FilePosn("see there"); }

        [UsedImplicitly]
        string DumpCurrent { get { return IsEnd ? "" : ("" + Current); } }

        const int DumpWidth = 10;

        [UsedImplicitly]
        string DumpAfterCurrent
        {
            get
            {
                if(IsEnd)
                    return "";
                var length = Math.Min(DumpWidth, Source.Length - Position - 1);
                var result = Source.SubString(Position + 1, length);
                if(length == DumpWidth)
                    result += "...";
                return result;
            }
        }

        [UsedImplicitly]
        string DumpBeforeCurrent
        {
            get
            {
                var start = Math.Max(0, Position - DumpWidth);
                var result = Source.SubString(start, Position - start);
                if(Position >= DumpWidth)
                    result = "..." + result;
                return result;
            }
        }
        public SourcePosn Clone { get { return new SourcePosn(Source, Position); } }

        public static SourcePosn operator +(SourcePosn x, int y) { return x._source + (x._position + y); }

        public static int operator -(SourcePosn x, SourcePosn y)
        {
            Tracer.Assert(x.Source == y.Source);
            return x.Position - y.Position;
        }

        public int? Match(IMatch automaton) { return automaton.Match(this); }

        public bool StartsWith(string data)
        {
            var length = data.Length;
            return !Source.IsEnd(Position + length - 1)
                && Source.SubString(Position, length) == data;
        }
    }
}