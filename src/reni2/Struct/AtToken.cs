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
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Struct
{
    sealed class AtToken : Infix
    {
        public override Result Result(ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right)
        {
            return left
                .AtTokenResult(callContext, category, right);
        }
        protected override IssueId LeftMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
        protected override IssueId RightMustNotBeNullError()
        {
            NotImplementedMethod();
            return null;
        }
    }
}