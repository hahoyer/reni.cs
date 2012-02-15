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

using HWClassLibrary.Debug;
using System.Collections.Generic;
using System.Linq;
using System;
using Reni.Parser;

namespace Reni.Syntax
{
    sealed class LeftParenthesis : ReniParser.ParsedSyntax
    {
        readonly int _leftLevel;
        readonly ReniParser.ParsedSyntax _left;
        readonly TokenClasses.LeftParenthesis _parenthesis;

        [EnableDump]
        readonly ReniParser.ParsedSyntax _right;

        public LeftParenthesis(int leftLevel, ReniParser.ParsedSyntax left, TokenClasses.LeftParenthesis parenthesis, TokenData token, ReniParser.ParsedSyntax right)
            : base(token)
        {
            _leftLevel = leftLevel;
            _left = left;
            _parenthesis = parenthesis;
            _right = right;
        }

        protected override TokenData GetFirstToken()
        {
            if(_left != null)
                return _left.LastToken;
            return base.GetFirstToken();
        }
        protected override TokenData GetLastToken()
        {
            if(_right != null)
                return _right.LastToken;
            return base.GetLastToken();
        }

        internal override ReniParser.ParsedSyntax RightParenthesis(int level, TokenData token)
        {
            if(level != _leftLevel)
                throw new ParenthesisMissmatchException(this, level, token);
            var surroundedByParenthesis = SurroundedByParenthesis(token);
            if(_left == null)
                return surroundedByParenthesis;
            return new InfixSyntax(token, _left.ToCompiledSyntax(), _parenthesis, _right.ToCompiledSyntax());
        }

        ReniParser.ParsedSyntax SurroundedByParenthesis(TokenData token) { return _right == null ? new EmptyList(Token, token) : _right.SurroundedByParenthesis(Token, token); }

        sealed class ParenthesisMissmatchException : Exception
        {
            readonly LeftParenthesis _leftParenthesis;
            readonly int _level;
            readonly TokenData _token;

            public ParenthesisMissmatchException(LeftParenthesis leftParenthesis, int level, TokenData token)
            {
                _leftParenthesis = leftParenthesis;
                _level = level;
                _token = token;
            }
        }
    }
}