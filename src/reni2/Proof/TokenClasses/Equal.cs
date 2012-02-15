// 
//     Project Reni2
//     Copyright (C) 2011 - 2012 Harald Hoyer
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

namespace Reni.Proof.TokenClasses
{
    sealed class Equal : PairToken
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null || right == null)
                return base.Syntax(left, token, right);
            return left.Equal(token, right);
        }

        protected override ParsedSyntax IsolateClause(string variable, ParsedSyntax left, ParsedSyntax right)
        {
            if(!left.Variables.Contains(variable))
            {
                if(right.Variables.Contains(variable))
                    return IsolateClause(variable, right, left);
                return null;
            }

            if(right.Variables.Contains(variable))
            {
                NotImplementedMethod(variable, left, right);
                return null;
            }

            return left.IsolateFromEquation(variable, right);
        }
    }
}