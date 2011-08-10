//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    internal sealed class Token : ReniObject
    {
        private readonly TokenData _data;
        private readonly ITokenClass _tokenClass;

        internal Token(ITokenClass tokenClass, Source source, int position, int length)
        {
            _data = new TokenData(source, position, length);
            _tokenClass = tokenClass;
        }

        internal TokenData Data { get { return _data; } }

        internal ITokenClass TokenClass { get { return _tokenClass; } }

        [DisableDump]
        public new string NodeDump { get { return ToString(); } }

        public override string ToString() { return Data.FilePosition; }

        public string ShortDump() { return Data.Name; }
        public override string DumpData() { return Data.Name; }

        [DisableDump]
        internal string PrioTableName { get { return TokenClass.PrioTableName(Data.Name); } }

        internal IParsedSyntax Syntax(IParsedSyntax left, IParsedSyntax right) { return TokenClass.Syntax(left, Data, right); }
    }

    internal sealed class TokenData : ReniObject
    {
        private static int _nextObjectId;
        private readonly int _length;
        private readonly Source _source;
        private readonly int _position;

        internal TokenData(Source source, int position, int length)
            : base(_nextObjectId++)
        {
            _source = source;
            _length = length;
            _position = position;
        }

        [DisableDump]
        internal Source Source { get { return _source; } }
        [DisableDump]
        internal int Position { get { return _position; } }
        [DisableDump]
        internal int Length { get { return _length; } }
        [DisableDump]
        internal string Name { get { return Source.SubString(Position, Length); } }

        internal string FilePosition { get { return "\n" + Source.FilePosn(Position, Name); } }
    }
}