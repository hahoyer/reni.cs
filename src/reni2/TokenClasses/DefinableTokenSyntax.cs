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
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    sealed class DefinableTokenSyntax : ParsedSyntax
    {
        readonly Defineable _defineable;

        internal DefinableTokenSyntax(Defineable defineable, TokenData tokenData)
            : base(tokenData) { _defineable = defineable; }

        internal override TokenData FirstToken { get { return Token; } }
        internal override TokenData LastToken { get { return Token; } }
        internal override ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right) { return new DeclarationSyntax(_defineable, token, right); }
        internal override ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return this; }
        internal override CompileSyntax ToCompiledSyntax() { return new ExpressionSyntax(_defineable, null, Token, null); }
    }
}