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
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class ArgToken : NonSuffix
    {
        protected override CompileSyntaxError LeftMustBeNull(ParsedSyntax left)
        {
            NotImplementedMethod(left);
            return null;
        }
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context
                .FindRecentFunctionContextObject
                .CreateArgReferenceResult(category);
        }
        public override Result Result(ContextBase context, Category category, TokenData token, CompileSyntax right)
        {
            var argTokenResult = Result(context, category.Typed, token);
            var rightResult = right.SmartReferenceResult(context, category.Typed);
            var feature = argTokenResult.Type.Feature;
            return feature
                       .Function
                       .ApplyResult(category, rightResult.Type)
                       .ReplaceArg(rightResult)
                   & category;
        }
    }
}