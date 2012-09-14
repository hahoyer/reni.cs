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
using HWClassLibrary.Debug;
using HWClassLibrary.Helper;
using Reni.Basics;
using Reni.Context;
using Reni.Feature;
using Reni.Parser;
using Reni.Struct;
using Reni.Syntax;
using Reni.Type;

namespace Reni.TokenClasses
{
    sealed class FunctionSyntax : SpecialSyntax
    {
        public readonly CompileSyntax Getter;
        readonly bool _isImplicit;
        readonly bool _isMetaFunction;
        public readonly CompileSyntax Setter;

        public FunctionSyntax(TokenData token, CompileSyntax setter, bool isImplicit, bool isMetaFunction, CompileSyntax getter)
            : base(token)
        {
            Getter = getter;
            Setter = setter;
            _isImplicit = isImplicit;
            _isMetaFunction = isMetaFunction;
        }

        string Tag
        {
            get
            {
                return (_isMetaFunction ? "{0}{0}" : "{0}")
                    .ReplaceArgs("/{0}\\")
                    .ReplaceArgs(IsImplicit ? "!" : "");
            }
        }

        internal override bool IsImplicit { get { return _isImplicit; } }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return context
                .FindRecentStructure
                .UniqueFunctionalType(this)
                .Result(category);
        }

        protected override bool GetIsLambda() { return true; }

        internal override string DumpPrintText
        {
            get
            {
                return
                    (Setter == null ? "" : Setter.DumpPrintText)
                    + Tag
                    + (Getter == null ? "" : Getter.DumpPrintText)
                    ;
            }
        }

        internal override string DumpShort()
        {
            var getter = Getter == null ? "" : "(" + Getter.DumpShort() + ")";
            var setter = Setter == null ? "" : "(" + Setter.DumpShort() + ")";
            return setter + base.DumpShort() + getter;
        }

        internal IMetaFunctionFeature MetaFunctionFeature(Structure structure)
        {
            if(!_isMetaFunction)
                return null;
            NotImplementedMethod(structure);
            return null;
        }
        internal IFunctionFeature FunctionFeature(Structure structure)
        {
            if(_isMetaFunction)
                return null;
            return new FunctionBodyType(structure, this);
        }
    }
}

namespace Reni.Type
{
}