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
using HWClassLibrary.TreeStructure;
using Reni.Basics;
using Reni.Context;
using Reni.Parser;

namespace Reni.Syntax
{
    [Serializable]
    abstract class SpecialSyntax : CompileSyntax
    {
        protected SpecialSyntax(TokenData token)
            : base(token) { }
    }

    [Serializable]
    sealed class TerminalSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        internal readonly ITerminal Terminal;

        public TerminalSyntax(TokenData token, ITerminal terminal)
            : base(token) { Terminal = terminal; }

        internal override string DumpPrintText { get { return Token.Name; } }
        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return Terminal
                .Result(context, category, Token);
        }

        protected override TokenData GetFirstToken() { return Token; }
        protected override TokenData GetLastToken() { return Token; }
    }

    [Serializable]
    sealed class PrefixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly IPrefix _prefix;

        [Node]
        [EnableDump]
        readonly CompileSyntax _right;

        public PrefixSyntax(TokenData token, IPrefix prefix, CompileSyntax right)
            : base(token)
        {
            _prefix = prefix;
            _right = right;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _prefix
                .Result(context, category, Token, _right);
        }

        protected override string GetNodeDump() { return base.GetNodeDump() + "(" + _right.NodeDump + ")"; }
        protected override TokenData GetFirstToken() { return Token; }
        protected override TokenData GetLastToken() { return _right.LastToken; }
    }

    [Serializable]
    sealed class InfixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly CompileSyntax _left;

        [Node]
        [EnableDump]
        readonly IInfix _infix;

        [Node]
        [EnableDump]
        readonly CompileSyntax _right;

        public InfixSyntax(TokenData token, CompileSyntax left, IInfix infix, CompileSyntax right)
            : base(token)
        {
            _left = left;
            _infix = infix;
            _right = right;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _infix
                .Result(context, category, _left, _right);
        }

        protected override string GetNodeDump()
        {
            var result = "(";
            result += _left.NodeDump;
            result += ")";
            result += base.GetNodeDump();
            result += "(";
            result += _right.NodeDump;
            result += ")";
            return result;
        }
        protected override TokenData GetFirstToken() { return _left.FirstToken; }
        protected override TokenData GetLastToken() { return _right.LastToken; }
    }

    sealed class SuffixSyntax : SpecialSyntax
    {
        [Node]
        [EnableDump]
        readonly CompileSyntax _left;

        [Node]
        [EnableDump]
        readonly ISuffix _suffix;

        internal SuffixSyntax(TokenData token, CompileSyntax left, ISuffix suffix)
            : base(token)
        {
            _left = left;
            _suffix = suffix;
        }

        internal override Result ObtainResult(ContextBase context, Category category)
        {
            return _suffix
                .Result(context, category, _left);
        }

        protected override string GetNodeDump() { return "(" + _left.NodeDump + ")" + base.GetNodeDump(); }

        protected override TokenData GetFirstToken() { return _left.FirstToken; }
        protected override TokenData GetLastToken() { return Token; }
    }

    interface ITerminal
    {
        Result Result(ContextBase context, Category category, TokenData token);
    }

    interface IPrefix
    {
        Result Result(ContextBase context, Category category, TokenData token, CompileSyntax right);
    }

    interface IInfix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left, CompileSyntax right);
    }

    interface ISuffix
    {
        Result Result(ContextBase context, Category category, CompileSyntax left);
    }
}