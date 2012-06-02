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
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    [Serializable]
    sealed class LeftParenthesis : TokenClass, IInfix
    {
        readonly int _level;

        internal LeftParenthesis(int level) { _level = level; }

        [DisableDump]
        internal int Level { get { return _level; } }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right) { return new Syntax.LeftParenthesis(_level, left, this, token, right); }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            var leftResult = left.Result(context, category.Typed);
            var rightResult = right.Result(context, category.Typed);
            return leftResult
                .Type
                .FunctionalFeatureSpecial
                .ApplyResult(category, rightResult, context.RefAlignParam)
                .ReplaceArg(leftResult);
        }
    }
}