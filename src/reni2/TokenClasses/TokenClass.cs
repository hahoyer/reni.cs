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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base clas for compiler tokens
    /// </summary>
    [Serializable]
    internal abstract class TokenClass : Parser.TokenClass
    {
        protected override IParsedSyntax Syntax(IParsedSyntax left, TokenData token, IParsedSyntax right) { return Syntax((ReniParser.ParsedSyntax) left, token, (ReniParser.ParsedSyntax) right); }

        protected virtual ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}