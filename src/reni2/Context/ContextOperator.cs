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
using Reni.Parser;
using Reni.Syntax;
using Reni.TokenClasses;

namespace Reni.Context
{
    internal sealed class ContextOperator : NonPrefix
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context
                .FindRecentStructure
                .StructReferenceViaContextReference(category);
        }

        public override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            StartMethodDump(false, context);
            try
            {
                BreakExecution();
                var leftResult = left.Result(context, category.Typed);
                Dump("leftResult", leftResult);
                BreakExecution();
                var structureType = leftResult.Type.FindRecentStructure;
                Dump("structureType", structureType);
                BreakExecution();
                if (structureType.StructIsDataLess)
                {
                    NotImplementedMethod(context,category,left);
                    return null;

                }
                var result = structureType.ReferenceType.Result(category, leftResult);
                return ReturnMethodDump(result, true);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}