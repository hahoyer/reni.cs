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
    sealed class Token : ReniObject
    {
        readonly TokenData _data;
        readonly ITokenClass _tokenClass;

        internal Token(ITokenClass tokenClass, Source source, int position, int length)
        {
            _data = new TokenData(source, position, length);
            _tokenClass = tokenClass;
        }

        TokenData Data { get { return _data; } }

        internal ITokenClass TokenClass { get { return _tokenClass; } }

        public override string ToString() { return Data.FilePosition; }
        public override string DumpData() { return Data.Name; }

        [DisableDump]
        internal string Name { get { return TokenClass.Name; } }

        internal IParsedSyntax Syntax(IParsedSyntax left, IParsedSyntax right = null) { return TokenClass.Syntax(left, Data, right); }

        internal static Token CreateAndAdvance(SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, ITokenClass tokenClass) { return CreateAndAdvance(sourcePosn, getLength, (sp, l) => tokenClass); }
        internal static Token CreateAndAdvance(SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<string, ITokenClass> getTokenClass) { return CreateAndAdvance(sourcePosn, getLength, (sp, l) => getTokenClass(sp.SubString(0, l))); }

        static Token CreateAndAdvance(SourcePosn sourcePosn, Func<SourcePosn, int?> getLength, Func<SourcePosn, int, ITokenClass> getTokenClass)
        {
            var length = getLength(sourcePosn);
            if(length == null)
                return null;

            var result = new Token(getTokenClass(sourcePosn, length.Value), sourcePosn.Source, sourcePosn.Position, length.Value);
            sourcePosn.Position += length.Value;
            return result;
        }
    }

    [DebuggerDisplay("{NodeDump} {DumpBeforeCurrent}[{DumpCurrent}]{DumpAfterCurrent}")]
    sealed class TokenData : ReniObject
    {
        static int _nextObjectId;
        readonly int _length;
        readonly Source _source;
        readonly int _position;

        internal TokenData(Source source, int position, int length)
            : base(_nextObjectId++)
        {
            _source = source;
            _length = length;
            _position = position;
            StopByObjectId(-2);
        }

        [DisableDump]
        Source Source { get { return _source; } }
        [DisableDump]
        int Position { get { return _position; } }
        [DisableDump]
        int Length { get { return _length; } }
        internal string Name { get { return Source.SubString(Position, Length); } }
        [DisableDump]
        internal string FilePosition { get { return "\n" + Source.FilePosn(Position, Name); } }
        internal string FileErrorPosition(string errorTag) { return "\n" + Source.FilePosn(Position, Name, "error " + errorTag); }

        [UsedImplicitly]
        string DumpCurrent { get { return Name; } }

        const int DumpWidth = 10;

        [UsedImplicitly]
        string DumpAfterCurrent
        {
            get
            {
                if(Source.IsEnd(Position + Length))
                    return "";
                var length = Math.Min(DumpWidth, Source.Length - Position - Length);
                var result = Source.SubString(Position + Length, length);
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
        public TokenData Combine(TokenData other)
        {
            Tracer.Assert(Source == other.Source);
            Tracer.Assert(Position + Length <= other.Position);
            return new TokenData(Source, Position, other.Position + other.Length - Position);
        }
    }
}