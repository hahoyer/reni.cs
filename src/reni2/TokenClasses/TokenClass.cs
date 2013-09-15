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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using HWClassLibrary.Parser;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    /// <summary>
    ///     Base clas for compiler tokens
    /// </summary>
    abstract class TokenClass : Parser.TokenClass, IOperator<ParsedSyntax>
    {
        protected override sealed IParsedSyntax Create
            (IParsedSyntax left
             , IPart<IParsedSyntax> token
             , IParsedSyntax right
            )
        {
            return
                this.Operation((ParsedSyntax)left, (TokenData)token, (ParsedSyntax)right);
        }

        ParsedSyntax IOperator<ParsedSyntax>.Terminal(IOperatorPart token) { return Terminal((TokenData)token); }
        ParsedSyntax IOperator<ParsedSyntax>.Prefix(IOperatorPart token, ParsedSyntax right) { return Prefix((TokenData)token, right); }
        ParsedSyntax IOperator<ParsedSyntax>.Suffix(ParsedSyntax left, IOperatorPart token) { return Suffix(left, (TokenData)token); }
        ParsedSyntax IOperator<ParsedSyntax>.Infix(ParsedSyntax left, IOperatorPart token, ParsedSyntax right) { return Infix(left, (TokenData)token, right); }

        protected virtual ParsedSyntax Syntax
            (ParsedSyntax left
             , TokenData token
             , ParsedSyntax right
            )
        {
            NotImplementedMethod(left, token, right);
            return null;
        }

        protected virtual ParsedSyntax Terminal(TokenData token)
        {
            NotImplementedMethod(token);
            return null;
        }

        protected virtual ParsedSyntax Prefix(TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(token, right);
            return null;
        }

        protected virtual ParsedSyntax Suffix(ParsedSyntax left, TokenData token)
        {
            NotImplementedMethod(left, token);
            return null;
        }

        protected virtual ParsedSyntax Infix(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            NotImplementedMethod(left, token, right);
            return null;
        }
    }
}