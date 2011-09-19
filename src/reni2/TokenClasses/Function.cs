//     Compiler for programming language "Reni"
//     Copyright (C) 2011 Harald Hoyer
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
using Reni.Syntax;

namespace Reni.TokenClasses
{
    abstract class Function : Suffix
    {
        readonly bool _isAutoCall;
        protected Function(bool isAutoCall) { _isAutoCall = isAutoCall; }
        [DisableDump]
        protected override bool IsLambda { get { return true; } }
        
        public override Result Result(ContextBase context, Category category, CompileSyntax target)
        {
            return context
                .FunctionalResult(category, target, _isAutoCall);
        }
    }

    sealed class CallableFunction : Function
    {
        public CallableFunction()
            : base(false) { }
    }

    sealed class AutoCallFunction : Function
    {
        public AutoCallFunction()
            : base(true) { }
    }
}