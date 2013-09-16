#region Copyright (C) 2013

//     Project Reni2
//     Copyright (C) 2011 - 2013 Harald Hoyer
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

using System.Collections.Generic;
using System.Linq;
using System;
using hw.Parser;
using Reni.Basics;
using Reni.ReniParser;
using Reni.Syntax;
using Reni.TokenClasses;
using Reni.Validation;

namespace Reni.Context
{
    sealed class ContextOperator : NonPrefix
    {
        protected override CompileSyntaxError RightMustBeNull(ParsedSyntax right)
        {
            NotImplementedMethod(right);
            return null;
        }
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            return context
                .FindRecentStructure
                .StructReferenceViaContextReference(category);
        }

        public override Result Result(ContextBase context, Category category, CompileSyntax left)
        {
            StartMethodDump(false, context, category, left);
            try
            {
                BreakExecution();
                var leftResult = left.Type(context);
                Dump("leftResult", leftResult);
                BreakExecution();

                var structure = leftResult.FindRecentStructure;
                Dump("structure", structure);
                BreakExecution();
                if(structure.IsDataLess)
                {
                    NotImplementedMethod(context, category, left);
                    return null;
                }
                var result = structure.PointerKind.Result(category, structure.ContainerContextObject);
                return ReturnMethodDump(result);
            }
            finally
            {
                EndMethodDump();
            }
        }
    }
}