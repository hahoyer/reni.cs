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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    [Serializable]
    sealed class Colon : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            left.AssertIsNotNull();
            return left.CreateDeclarationSyntax(token, right);
        }
    }

    [Serializable]
    sealed class Exclamation : TokenClass
    {
        static readonly ITokenFactory _tokenFactory = DeclarationTokenFactory.Instance;

        [DisableDump]
        protected override ITokenFactory NewTokenFactory { get { return _tokenFactory; } }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            left.AssertIsNull();
            right.AssertIsNull();
            return new ExclamationSyntax(token);
        }
    }

    [Serializable]
    sealed class ConverterToken : TokenClass
    {
        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            right.AssertIsNull();
            return ((DeclarationExtensionSyntax) left).ExtendByConverter(token);
        }
    }

    abstract class DeclarationExtensionSyntax : ReniParser.ParsedSyntax
    {
        protected DeclarationExtensionSyntax(TokenData token)
            : base(token) { }

        internal virtual ReniParser.ParsedSyntax ExtendByConverter(TokenData token)
        {
            NotImplementedMethod(token);
            return null;
        }
    }

    sealed class ConverterDeclarationSyntax : DeclarationExtensionSyntax
    {
        readonly TokenData _token;

        internal ConverterDeclarationSyntax(TokenData token, TokenData otherToken)
            : base(token) { _token = otherToken; }

        internal override ReniParser.ParsedSyntax CreateDeclarationSyntax(TokenData token, ReniParser.ParsedSyntax right) { return new ConverterSyntax(_token, right.CheckedToCompiledSyntax()); }
    }

    sealed class ExclamationSyntax : DeclarationExtensionSyntax
    {
        internal ExclamationSyntax(TokenData token)
            : base(token) { }

        internal override ReniParser.ParsedSyntax ExtendByConverter(TokenData token) { return new ConverterDeclarationSyntax(Token, token); }
    }
}