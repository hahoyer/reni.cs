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

using hw.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using hw.Parser;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.TokenClasses
{
    sealed class Text : TerminalToken
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            var data = StripQutes(token.Name);
            return context
                .RootContext.BitType.UniqueNumber(BitsConst.BitSize(data[0].GetType()))
                .UniqueTextItemType
                .UniqueArray(data.Length)
                .UniqueTextItemsType
                .Result(category, () => CodeBase.BitsConst(BitsConst.ConvertAsText(data)), CodeArgs.Void);
        }

        protected override CompileSyntaxError LeftAndRightMustBeNull(ParsedSyntax left, ParsedSyntax right)
        {
            NotImplementedMethod(left, right);
            return null;
        }
        
        static string StripQutes(string text)
        {
            var result = "";
            for(var i = 1; i < text.Length - 1; i++)
            {
                result += text[i];
                if(text[i] == text[0])
                    i++;
            }
            return result;
        }
    }
}