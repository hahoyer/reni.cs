﻿//     Compiler for programming language "Reni"
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Code;
using Reni.Context;
using Reni.Parser;
using Reni.Type;

namespace Reni.TokenClasses
{
    internal sealed class Number : Terminal
    {
        public override Result Result(ContextBase context, Category category, TokenData token)
        {
            var bitsConst = BitsConst.Convert(token.Name);
            return TypeBase
                .UniqueNumber(bitsConst.Size.ToInt())
                .Result(category, () => CodeBase.BitsConst(bitsConst), CodeArgs.Void);
        }

        public static Int64 ToInt64(TokenData token) { return BitsConst.Convert(token.Name).ToInt64(); }
    }
}