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
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Syntax;

namespace Reni.Context
{
    internal sealed class PendingContext : Child
    {
        internal PendingContext(ContextBase parent)
            : base(parent) { }

        protected override Result PendingResult(Category category, CompileSyntax syntax)
        {
            if(category.HasCode)
            {
                NotImplementedMethod(category, syntax);
                return null;
            }

            if (category.HasIsDataLess)
            {
                NotImplementedMethod(category, syntax);
                return null;
            }

            var result = new Result();
            var reducedCategory = category - Category.Args;
            if(reducedCategory.HasAny)
                result.Update(syntax.ObtainResult(this, reducedCategory));
            if(category.HasArgs)
                result.CodeArgs = CodeArgs.Void();

            Tracer.Assert(result.CompleteCategory == category);
            return result;
        }

        protected override Result CommonResult(Category category, CondSyntax condSyntax)
        {
            var pendingCategory = Parent.PendingCategory(condSyntax);
            if(category <= pendingCategory)
            {
                return condSyntax.CommonResult
                    (
                        this,
                        category,
                        category <= Parent.PendingCategory(condSyntax.Then),
                        condSyntax.Else != null && category <= Parent.PendingCategory(condSyntax.Else)
                    );
            }
            NotImplementedMethod(category, condSyntax);
            return null;
        }
    }
}
