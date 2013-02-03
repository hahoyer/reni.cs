﻿#region Copyright (C) 2013

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

        [DisableDump]
        protected string DataFunctionName { get { return Name.Symbolize(); } }
        string ISearchTarget.StructFeatureName { get { return Name; } }
        IEnumerable<TPath> ISearchTarget.GetFeature<TPath>(TypeBase typeBase)
        {
            var result = GetFeature<TPath>(typeBase);
            if(result == null)
                yield break;
            yield return result;
        }
        protected virtual TPath GetFeature<TPath>(TypeBase typeBase) where TPath : class { return null; }
    }

    abstract class Defineable<TTarget> : Defineable
        where TTarget : class
    {
        protected override TPath GetFeature<TPath>(TypeBase provider)
        {
            return provider.GetFeature<TPath, TTarget>(this as TTarget)
                ?? base.GetFeature<TPath>(provider);
        }
    }
}