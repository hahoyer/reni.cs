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
using Reni.Parser;
using Reni.ReniParser;

namespace Reni.Syntax
{
    sealed class EmptyList : CompileSyntax
    {
        readonly TokenData _rightToken;

        public EmptyList(TokenData leftToken, TokenData rightToken)
            : base(leftToken) { _rightToken = rightToken; }

        internal override TokenData LastToken { get { return _rightToken; } }
        protected override string GetNodeDump() { return "()"; }

        internal override Result ObtainResult(ContextBase context, Category category) { return context.RootContext.VoidResult(category); }

        internal sealed class Half : CompileSyntax
        {
            public Half(TokenData leftToken)
                : base(leftToken) { }

            internal override TokenData LastToken { get { return FirstToken; } }
            protected override string GetNodeDump() { return "("; }
            internal override ParsedSyntax RightParenthesis(int level, TokenData token) { return new EmptyList(Token, token); }
        }

    }
}