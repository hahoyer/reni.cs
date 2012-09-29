#region Copyright (C) 2012

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.Type;

namespace Reni.TokenClasses
{
    [Serializable]
    sealed class LeftParenthesis : TokenClass, IInfix
    {
        readonly int _level;

        internal LeftParenthesis(int level) { _level = level; }

        [DisableDump]
        internal int Level { get { return _level; } }

        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right) { return new Syntax.LeftParenthesis(_level, left, this, token, right); }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return FeatureInstance.ObjectResult(context, category, left, right);
        }
    }
}