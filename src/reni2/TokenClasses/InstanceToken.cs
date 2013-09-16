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
using hw.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Syntax;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class InstanceToken : Infix, IPendingProvider
    {
        protected override Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            return left
                .Type(context)
                .InstanceResult(category, c => context.ResultAsReference(c, right));
        }

        Result IPendingProvider.ObtainResult(ContextBase context, Category category, CompileSyntax left, CompileSyntax right)
        {
            if(category <= Category.Type.Replenished)
                return Result(context, category, left, right);
            NotImplementedMethod(context, category, left, right);
            return null;
        }

    }
}