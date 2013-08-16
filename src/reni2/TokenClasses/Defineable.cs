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
using Reni.Feature;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.Type;

namespace Reni.TokenClasses
{
    abstract class Defineable : TokenClass, ISearchTarget
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            if(left == null && right == null)
                return new DefinableTokenSyntax(this, token);
            if(left == null)
                return new ExpressionSyntax(this, null, token, right.ToCompiledSyntaxOrNull());
            return left.CreateSyntaxOrDeclaration(this, token, right);
        }
        sealed protected override ParsedSyntax Terminal(TokenData token) { return new DefinableTokenSyntax(this, token); }
        sealed protected override ParsedSyntax Prefix(TokenData token, ParsedSyntax right) { return new ExpressionSyntax(this, null, token, (CompileSyntax)right); ; }
        sealed protected override ParsedSyntax Suffix(ParsedSyntax left, TokenData token) { return left.CreateSyntaxOrDeclaration(this, token, null); }
        sealed protected override ParsedSyntax Infix(ParsedSyntax left, TokenData token, ParsedSyntax right) { return left.CreateSyntaxOrDeclaration(this, token, right); }

        [DisableDump]
        protected string DataFunctionName { get { return Name.Symbolize(); } }
        string ISearchTarget.StructFeatureName { get { return Name; } }
        IFeatureImplementation ISearchTarget.GetFeature(IFeatureProvider provider) { return GetFeature(provider); }
        protected virtual IFeatureImplementation GetFeature(IFeatureProvider typeBase) { return null; }
    }

    abstract class Defineable<TTarget> : Defineable
        where TTarget : Defineable
    {
        protected override IFeatureImplementation GetFeature(IFeatureProvider provider)
        {
            return provider.GetFeature(this as TTarget)
                ?? base.GetFeature(provider);
        }
    }
}