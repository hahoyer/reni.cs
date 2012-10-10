//     Compiler for programming language "Reni"
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
using Reni.Context;
using Reni.ReniParser;
using Reni.Validation;

namespace Reni.Parser
{
    [Serializable]
    internal sealed class Scanner : ReniObject, IScanner
    {
        private readonly char[] _charType = new char[256];

        /// <summary>
        ///     ctor
        /// </summary>
        public Scanner() { InitCharType(); }

        private void InitCharType()
        {
            _charType[0] = '?';
            for(var i = 1; i < 256; i++)
                _charType[i] = '*';
            SetCharType(' ', " \t\n\r");
            SetCharType('0', "0123456789");
            SetCharType('a', "qwertzuiopasdfghjklyxcvbnmQWERTZUIOPASDFGHJKLYXCVBNM_");
            SetCharType('?', "#'\"({[)}];,");
        }

        private bool IsDigit(char @char) { return _charType[@char] == '0'; }

        private bool IsAlpha(char @char) { return _charType[@char] == 'a'; }

        private bool IsSymbol(char @char) { return _charType[@char] == '*'; }

        private bool IsWhiteSpace(char @char) { return _charType[@char] == ' '; }

        private bool IsAlphaNum(char @char) { return IsAlpha(@char) || IsDigit(@char); }

        /// <summary>
        ///     Scans source for begin of next token, advances and returns the new token.
        /// </summary>
        /// <param name = "sourcePosn">Source position, is advanced during create token</param>
        /// <param name = "tokenFactory">The token factory.</param>
        /// <returns>the next token</returns>
        Token IScanner.CreateToken(SourcePosn sourcePosn, ITokenFactory tokenFactory)
        {
            for(;;)
            {
                WhiteSpace(sourcePosn);

                if(sourcePosn.IsEnd())
                    return Token(tokenFactory.RightParenthesisClass(0), sourcePosn, 0);
                if(IsDigit(sourcePosn.Current))
                    return Number(sourcePosn, tokenFactory);
                if(IsAlpha(sourcePosn.Current))
                    return Name(sourcePosn, tokenFactory);
                if(IsSymbol(sourcePosn.Current))
                    return Symbol(sourcePosn, tokenFactory);

                switch(sourcePosn.Current)
                {
                    case '#':
                    {
                        var error = Comment(sourcePosn);
                        if(error != null)
                            return error;
                        break;
                    }

                    case '"':
                        return Text(sourcePosn, tokenFactory);
                    case '\'':
                        return Text(sourcePosn, tokenFactory);

                    case '(':
                        return Token(tokenFactory.LeftParenthesisClass(3), sourcePosn, 1);
                    case '[':
                        return Token(tokenFactory.LeftParenthesisClass(2), sourcePosn, 1);
                    case '{':
                        return Token(tokenFactory.LeftParenthesisClass(1), sourcePosn, 1);

                    case ')':
                        return Token(tokenFactory.RightParenthesisClass(3), sourcePosn, 1);
                    case ']':
                        return Token(tokenFactory.RightParenthesisClass(2), sourcePosn, 1);
                    case '}':
                        return Token(tokenFactory.RightParenthesisClass(1), sourcePosn, 1);

                    case ';':
                    case ',':
                        return Token(tokenFactory.ListClass, sourcePosn, 1);
                    default:
                        DumpMethodWithBreak("not implemented", sourcePosn);
                        throw new NotImplementedException();
                }
            }
        }

        private static Token Token(ITokenClass tokenClass, SourcePosn sourcePosn, int length)
        {
            var result = new Token(tokenClass, sourcePosn.Source, sourcePosn.Position, length);
            sourcePosn.Incr(length);
            return result;
        }

        private void WhiteSpace(SourcePosn sp)
        {
            var i = 0;
            while(IsWhiteSpace(sp[i]))
                i++;
            sp.Incr(i);
        }

        private Token Number(SourcePosn sp, ITokenFactory tokenFactory)
        {
            var i = 1;
            while(IsDigit(sp[i]))
                i++;
            return Token(tokenFactory.NumberClass, sp, i);
        }

        private Token Name(SourcePosn sp, ITokenFactory tokenFactory)
        {
            var i = 1;
            while(IsAlphaNum(sp[i]))
                i++;
            return Token(tokenFactory.TokenClass(sp.SubString(0, i)), sp, i);
        }

        private Token Symbol(SourcePosn sp, ITokenFactory tokenFactory)
        {
            var i = 1;
            while(IsSymbol(sp[i]))
                i++;
            return Token(tokenFactory.TokenClass(sp.SubString(0, i)), sp, i);
        }

        private Token Comment(SourcePosn sp)
        {
            var closingParenthesis = "?)}]"["({[".IndexOf(sp[1]) + 1];

            if(closingParenthesis == '?')
            {
                SingleLineComment(sp);
                return null;
            }

            int? errorPosition = null;
            var i = 3;
            var endOfComment = closingParenthesis + "#";
            if(IsSymbol(sp[2]) || sp[2] == '#')
                endOfComment = sp[2] + endOfComment;
            else if(IsAlpha(sp[2]))
            {
                while(IsAlphaNum(sp[i]))
                    i++;
                endOfComment = sp.SubString(2, i - 2) + endOfComment;
            }
            else
                errorPosition = 2;


            while(!(sp[i] == '\0' || IsWhiteSpace(sp[i]) && sp.SubString(i + 1, endOfComment.Length) == endOfComment))
                i++;
            if(sp[i] == '\0')
                return Token(_syntaxErrorEOFComment, sp, i);

            var commentEndPosition = i + 1 + endOfComment.Length;
            if(errorPosition == null)
            {
                sp.Incr(commentEndPosition);
                return null;
            }
            return Token(_syntaxErrorBeginComment, sp, commentEndPosition);
        }

        private static void SingleLineComment(SourcePosn sp)
        {
            var i = 1;
            while(sp[i] != '\0' && sp[i] != '\n')
                i++;
            sp.Incr(i);
        }

        private static readonly SyntaxError _syntaxErrorEOFComment = new SyntaxError(IssueId.EOFInComment, true);
        private static readonly SyntaxError _syntaxErrorBeginComment = new SyntaxError(IssueId.BeginOfComment, true);
        private static readonly SyntaxError _syntaxErrorEOLString = new SyntaxError(IssueId.EOLInString, false);

        private static Token Text(SourcePosn sourcePosn, ITokenFactory tokenFactory)
        {
            var position = 1;
            while(sourcePosn[position] != '\0')
            {
                position++;
                if (sourcePosn[position - 1] == sourcePosn[0])
                {
                    if (sourcePosn[position] != sourcePosn[0])
                        return Token(tokenFactory.TextClass, sourcePosn, position);

                    position++;
                }
            }

            return Token(_syntaxErrorEOLString, sourcePosn, position);
        }

        private void SetCharType(char type, IEnumerable<char> chars)
        {
            foreach(var t in chars)
                _charType[t] = type;
        }
    }

    class CommentError : SyntaxIssue
    {
        public CommentError(ParsedSyntaxBase syntax, IssueId issueId)
            : base(syntax, issueId) {}
    }
}