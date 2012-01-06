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

using HWClassLibrary.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    abstract class Function : Special, IInfix
    {
        readonly bool _isAutoCall;
        protected Function(bool isAutoCall) { _isAutoCall = isAutoCall; }

        [DisableDump]
        bool IInfix.IsLambda { get { return true; } }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            Tracer.Assert(left != null || right != null);
            return new InfixSyntax(token, left.ToCompiledSyntaxOrNull(), this, right.ToCompiledSyntaxOrNull());
        }

        Result IInfix.Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return context
                .FunctionalResult(category, left, right, _isAutoCall);
        }
    }

    sealed class ExplicitFunction : Function
    {
        public ExplicitFunction()
            : base(false) { }
    }

    sealed class ImplicitFunction : Function
    {
        public ImplicitFunction()
            : base(true) { }
    }
}