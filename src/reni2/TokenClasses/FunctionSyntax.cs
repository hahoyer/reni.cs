#region Copyright (C) 2012

// 
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
using HWClassLibrary.Debug;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    sealed class FunctionSyntax : SpecialSyntax
    {
        public readonly CompileSyntax Getter;
        internal readonly bool IsImplicit;
        public readonly CompileSyntax Setter;

        public FunctionSyntax(TokenData token, CompileSyntax getter, bool isImplicit, CompileSyntax setter)
            : base(token)
        {
            Getter = getter;
            IsImplicit = isImplicit;
            Setter = setter;
        }

        internal override Result ObtainResult(ContextBase context, Category category) { return context.FunctionalResult(category, this); }

        string Tag { get { return IsImplicit ? "/!\\" : "/\\"; } }
        internal override string DumpPrintText
        {
            get
            {
                return
                    (Getter == null ? "" : Getter.DumpPrintText)
                    + Tag
                    + (Setter == null ? "" : Setter.DumpPrintText);
            }
        }

        internal override string DumpShort()
        {
            var getter = Getter == null ? "" : "(" + Getter.DumpShort() + ")";
            var setter = Setter == null ? "" : "(" + Setter.DumpShort() + ")";
            return getter + base.DumpShort() + setter;
        }
    }
}