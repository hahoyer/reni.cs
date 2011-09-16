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
using Reni.Context;
using Reni.Parser;
using Reni.ReniParser;
using Reni.Syntax;

namespace Reni.TokenClasses
{
    [Serializable]
    abstract class Special : TokenClass
    {}

    abstract class Terminal : Special, ITerminal
    {
        protected override sealed ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            left.AssertIsNull();
            right.AssertIsNull();
            return new TerminalSyntax(token, this);
        }

        public abstract Result Result(ContextBase context, Category category, TokenData token);
        public abstract bool? QuickIsDereferencedDataLess(ContextBase context, TokenData token);
    }

    abstract class NonPrefix : Special, ITerminal, ISuffix
    {
        [DisableDump]
        bool ISuffix.IsLambda { get { return false; } }

        protected override sealed ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            right.AssertIsNull();
            if(left == null)
                return new TerminalSyntax(token, this);
            return new SuffixSyntax(token, left.CheckedToCompiledSyntax(), this);
        }

        public abstract Result Result(ContextBase context, Category category, TokenData token);
        public abstract Result Result(ContextBase context, Category category, CompileSyntax left);
        bool? ITerminal.QuickIsDereferencedDataLess(ContextBase context, TokenData token)
        {
            NotImplementedMethod(context);
            return null;
        }
    }

    [Serializable]
    abstract class Prefix : Special, IPrefix
    {
        public abstract Result Result(ContextBase context, Category category, CompileSyntax right);

        protected override sealed ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            left.AssertIsNull();
            return new PrefixSyntax(token, this, right.CheckedToCompiledSyntax());
        }
    }

    [Serializable]
    abstract class Suffix : Special, ISuffix
    {
        [DisableDump]
        bool ISuffix.IsLambda { get { return IsLambda; } }
        [DisableDump]
        protected virtual bool IsLambda { get { return false; } }
        public abstract Result Result(ContextBase context, Category category, CompileSyntax right);

        protected override sealed ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right)
        {
            right.AssertIsNull();
            return new SuffixSyntax(token, left.CheckedToCompiledSyntax(), this);
        }
    }

    [Serializable]
    abstract class Infix : Special, IInfix
    {
        public abstract Result Result(ContextBase callContext, Category category, CompileSyntax left, CompileSyntax right);

        protected override sealed ReniParser.ParsedSyntax Syntax(ReniParser.ParsedSyntax left, TokenData token, ReniParser.ParsedSyntax right) { return new InfixSyntax(token, left.CheckedToCompiledSyntax(), this, right.CheckedToCompiledSyntax()); }
    }
}