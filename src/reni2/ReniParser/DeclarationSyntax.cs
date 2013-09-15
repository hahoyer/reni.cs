// 
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

using System;
using System.Collections.Generic;
using System.Linq;
using HWClassLibrary.Debug;
using Reni.Parser;
using Reni.Struct;
using Reni.TokenClasses;

namespace Reni.ReniParser
{
    sealed class DeclarationSyntax : ParsedSyntax
    {
        internal readonly Defineable Defineable;
        internal readonly ParsedSyntax Definition;

        internal DeclarationSyntax(Defineable defineable, TokenData token, ParsedSyntax definition)
            : base(token)
        {
            Defineable = defineable;
            Definition = definition;
        }

        protected override string GetNodeDump() { return Defineable.Name + ": " + Definition.NodeDump; }

        internal override ParsedSyntax SurroundedByParenthesis(TokenData leftToken, TokenData rightToken) { return Container.Create(leftToken, rightToken, this); }
    }
}