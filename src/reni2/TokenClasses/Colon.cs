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
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Colon : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null)
                return LeftMustNotBeNullError();
            return left.CreateDeclarationSyntax(token, right);
        }
        CompileSyntaxError LeftMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }

    sealed class Exclamation : TokenClass
    {
        static readonly ITokenFactory _tokenFactory = DeclarationTokenFactory.Instance;

        [DisableDump]
        protected override ITokenFactory NewTokenFactory { get { return _tokenFactory; } }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left != null)
                return LeftMustBeNullError(left);
            if(right != null)
                return RightMustBeNullError(right);
            return new ExclamationSyntax(token);
        }
        CompileSyntaxError LeftMustBeNullError(ParsedSyntax left)
        {
            NotImplementedMethod(left);
            return null;
        }
        CompileSyntaxError RightMustBeNullError(ParsedSyntax rigth)
        {
            NotImplementedMethod(rigth);
            return null;
        }
    }

    sealed class ConverterToken : TokenClass
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(right != null)
                return RightMustBeNullError(right);
            return ((DeclarationExtensionSyntax) left).ExtendByConverter(token);
        }
        CompileSyntaxError RightMustBeNullError(ParsedSyntax right)
        {
            NotImplementedMethod(right);
            return null;
        }
    }

    abstract class DeclarationExtensionSyntax : ParsedSyntax
    {
        protected DeclarationExtensionSyntax(TokenData token)
            : base(token) { }

        internal virtual ParsedSyntax ExtendByConverter(TokenData token)
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

        internal override ParsedSyntax CreateDeclarationSyntax(TokenData token, ParsedSyntax right)
        {
            return new ConverterSyntax
                (_token
                    , right.CheckedToCompiledSyntax(token, RightMustNotBeNullError)
                );
        }
        IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }

    sealed class ExclamationSyntax : DeclarationExtensionSyntax
    {
        internal ExclamationSyntax(TokenData token)
            : base(token) { }

        internal override ParsedSyntax ExtendByConverter(TokenData token)
        {
            return new ConverterDeclarationSyntax
                (Token
                    , token
                );
        }
    }
}