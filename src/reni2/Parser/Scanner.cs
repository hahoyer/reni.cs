#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;

namespace Reni.Parser
{
    abstract class Scanner : ReniObject
    {
        protected abstract int WhiteSpace(SourcePosn sourcePosn);
        protected abstract int? Number(SourcePosn sourcePosn);
        protected abstract int? Text(SourcePosn sourcePosn);
        protected abstract int? Any(SourcePosn sourcePosn);

        internal Token CreateToken(SourcePosn sourcePosn, ITokenFactory tokenFactory)
        {
            try
            {
                sourcePosn.Incr(WhiteSpace(sourcePosn));
                return Token.CreateAndAdvance(sourcePosn, sp => sp.IsEnd ? (int?) 0 : null, tokenFactory.EndOfText)
                       ?? Token.CreateAndAdvance(sourcePosn, Number, tokenFactory.Number)
                       ?? Token.CreateAndAdvance(sourcePosn, Text, tokenFactory.Text)
                       ?? Token.CreateAndAdvance(sourcePosn, Any, tokenFactory.TokenClass)
                       ?? WillReturnNull(sourcePosn);
            }
            catch(Exception exception)
            {
                return Token.CreateAndAdvance(exception.SourcePosn, sp => exception.Length, exception.TokenClass);
            }
        }
        Token WillReturnNull(SourcePosn sourcePosn)
        {
            NotImplementedMethod(sourcePosn);
            return null;
        }

        internal sealed class Exception : System.Exception
        {
            public readonly SourcePosn SourcePosn;
            public readonly ITokenClass TokenClass;
            public readonly int Length;

            public Exception(SourcePosn sourcePosn, ITokenClass tokenClass, int length)
            {
                SourcePosn = sourcePosn;
                TokenClass = tokenClass;
                Length = length;
            }
        }
    }
}