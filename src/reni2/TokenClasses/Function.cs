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

using HWClassLibrary.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.TokenClasses
{
    abstract class Function : Special
    {
        readonly bool _isAutoCall;
        protected Function(bool isAutoCall) { _isAutoCall = isAutoCall; }

        protected override ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right) { return new FunctionSyntax(token, left.ToCompiledSyntaxOrNull(), _isAutoCall, right.ToCompiledSyntaxOrNull()); }
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