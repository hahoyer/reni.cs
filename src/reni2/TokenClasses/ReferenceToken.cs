#region Copyright (C) 2012

//     Project Reni2
//     Copyright (C) 2012 - 2012 Harald Hoyer
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

using System.Linq;
using System.Collections.Generic;
using System;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    sealed class ReferenceToken : Special, ISuffix, IInfix
    {
        protected override ParsedSyntax Syntax(ParsedSyntax left, TokenData token, ParsedSyntax right)
        {
            var leftSyntax = left.CheckedToCompiledSyntax(token, LeftMustNotBeNullError);
            if(right == null)
                return new SuffixSyntax
                    (token
                     , leftSyntax
                     , this
                    );
            return new InfixSyntax
                (token
                 , leftSyntax
                 , this
                 , right.ToCompiledSyntax()
                );
        }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            if(context.Type(right) == context.RootContext.VoidType)
                return Result(context, category, left, null);
            return Result(context, category, left, right.Evaluate(context).ToInt32());
        }
        Result ISuffix.Result(ContextBase context, Category category, CompileSyntax left) { return Result(context, category, left, 1); }

        static Result Result(ContextBase context, Category category, CompileSyntax left, int? count)
        {
            var leftType = left.Type(context).TypeForTypeOperator;
            return leftType.CreateReference(context, category, left, count);
        }
    }
}